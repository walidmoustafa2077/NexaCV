using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexaCV.Identity.Models;
using NexaCV.Identity.Settings;

namespace NexaCV.Identity.Services;

/// <summary>
/// Responsible solely for generating cryptographically sound
/// Access Tokens (JWT) and Refresh Tokens (opaque random bytes).
/// Business rules (rotation, revocation, persistence) live in AuthService.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    private readonly RefreshTokenSettings _refresh;

    public TokenService(
        IOptions<JwtSettings> jwt,
        IOptions<RefreshTokenSettings> refresh)
    {
        _jwt = jwt.Value;
        _refresh = refresh.Value;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_jwt.AccessTokenExpiresInSeconds),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress)
    {
        // Cryptographically random, URL-safe token
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var tokenString = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            Token = tokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_refresh.ExpiresInDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
}
