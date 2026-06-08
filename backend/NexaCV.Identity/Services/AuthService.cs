using Microsoft.EntityFrameworkCore;
using NexaCV.Identity.Data;
using NexaCV.Identity.DTOs;
using NexaCV.Identity.Models;

namespace NexaCV.Identity.Services;

/// <summary>
/// Handles all authentication flows: register, login, refresh token rotation,
/// and explicit revocation. Refresh Token Rotation ensures that each token
/// can only be used once; reuse triggers a "family revocation" security measure.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IdentityDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(IdentityDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent = null)
    {
        var emailLower = request.Email.ToLowerInvariant();
        var usernameLower = request.Username.ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == emailLower))
            throw new InvalidOperationException("A user with that email already exists.");

        if (await _db.Users.AnyAsync(u => u.Username == usernameLower))
            throw new InvalidOperationException("A user with that username already exists.");

        var user = new ApplicationUser
        {
            Email = emailLower,
            Username = usernameLower,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        LogSecurityEvent(user.Id, SecurityEventType.Registration, ipAddress, userAgent);

        return await IssueTokenPairAsync(user, ipAddress, userAgent);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent = null)
    {
        var identifier = request.EmailOrUsername.ToLowerInvariant();

        // Supports login by email OR username
        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);

        if (user is null)
        {
            LogSecurityEvent(identifier, SecurityEventType.LoginFailure, ipAddress, userAgent);
            throw new UnauthorizedAccessException("Invalid email/username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            LogSecurityEvent(identifier, SecurityEventType.LoginFailure, ipAddress, userAgent);
            throw new UnauthorizedAccessException("Invalid email/username or password.");
        }

        user.LastLogin = DateTime.UtcNow;
        LogSecurityEvent(user.Id, SecurityEventType.LoginSuccess, ipAddress, userAgent);
        await _db.SaveChangesAsync();

        return await IssueTokenPairAsync(user, ipAddress, userAgent);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent = null)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.User)
                .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(r => r.Token == refreshToken)
            ?? throw new SecurityException("Invalid refresh token.");

        // --- Reuse Detection ---
        // If the token is already revoked it was either used before or explicitly revoked.
        // Either way, someone is attempting replay — revoke the entire family as a precaution.
        if (token.IsRevoked)
        {
            LogSecurityEvent(token.User.Id, SecurityEventType.TokenRefreshSuspicious, ipAddress, userAgent);
            RevokeTokenFamily(token.User, "Reuse of revoked token detected.");
            await _db.SaveChangesAsync();
            throw new SecurityException("Refresh token has been revoked. All sessions for this user have been terminated.");
        }

        if (token.IsExpired)
            throw new SecurityException("Refresh token has expired.");

        // --- Rotation ---
        var newRefreshToken = _tokenService.GenerateRefreshToken(token.UserId, ipAddress);
        _db.RefreshTokens.Add(newRefreshToken);

        // Revoke the old token, linking to the replacement for audit trail
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = newRefreshToken.Token;
        token.RevocationReason = "Replaced by new token on refresh.";

        // Clean up old expired/revoked tokens for this user (housekeeping)
        RemoveExpiredTokens(token.User);

        LogSecurityEvent(token.User.Id, SecurityEventType.TokenRefresh, ipAddress, userAgent);
        await _db.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(token.User);

        return new AuthResponse(
            token.User.Id,
            accessToken,
            DateTime.UtcNow.AddSeconds(900),  // mirrors JwtSettings default
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt);
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress, string? userAgent = null)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken)
            ?? throw new SecurityException("Invalid refresh token.");

        if (!token.IsActive)
            throw new InvalidOperationException("Token is already inactive.");

        LogSecurityEvent(token.UserId, SecurityEventType.Logout, ipAddress, userAgent);

        token.RevokedAt = DateTime.UtcNow;
        token.RevocationReason = $"Explicitly revoked by {ipAddress ?? "unknown"}.";

        await _db.SaveChangesAsync();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<AuthResponse> IssueTokenPairAsync(ApplicationUser user, string? ipAddress, string? userAgent = null)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, ipAddress);

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse(
            user.Id,
            accessToken,
            DateTime.UtcNow.AddSeconds(900),
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }

    /// <summary>
    /// Revokes all active Refresh Tokens belonging to the user.
    /// Called when token reuse is detected — indicates possible compromise.
    /// </summary>
    private static void RevokeTokenFamily(ApplicationUser user, string reason)
    {
        foreach (var t in user.RefreshTokens.Where(t => t.IsActive))
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevocationReason = reason;
        }
    }

    private static void RemoveExpiredTokens(ApplicationUser user)
    {
        var stale = user.RefreshTokens
            .Where(t => !t.IsActive && t.CreatedAt < DateTime.UtcNow.AddDays(-30))
            .ToList();

        foreach (var t in stale)
            user.RefreshTokens.Remove(t);
    }

    private void LogSecurityEvent(Guid userId, SecurityEventType action, string? ipAddress, string? userAgent = null)
    {
        _db.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            UserId = userId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });
    }

    /// <summary>
    /// Logs a login-failure event when the user doesn't exist yet (no GUID available).
    /// </summary>
    private void LogSecurityEvent(string attemptedIdentifier, SecurityEventType action, string? ipAddress, string? userAgent = null)
    {
        _db.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            UserId = Guid.Empty,
            Action = action,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });
    }
}

/// <summary>Thrown on deliberate security violations (token reuse, invalid tokens).</summary>
public sealed class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
}
