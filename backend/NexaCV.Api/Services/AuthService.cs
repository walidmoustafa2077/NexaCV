using NexaCV.Api.DTOs.Auth;
using NexaCV.Api.Enums;
using NexaCV.Api.Extensions;
using NexaCV.Api.Repositories;

namespace NexaCV.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUserMovementRepository _movements;
    private readonly JwtService _jwt;

    public AuthService(
        IUserRepository users,
        IUserMovementRepository movements,
        JwtService jwt)
    {
        _users = users;
        _movements = movements;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, string? ip, string? userAgent)
    {
        if (await _users.ExistsByEmailOrUsernameAsync(req.Email, req.Username))
            throw new ConflictException("A user with that email or username already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = req.ToUser(passwordHash);

        await _users.AddAsync(user);

        var token = _jwt.GenerateToken(user);

        return new AuthResponse { UserId = user.Id, Token = token, ExpiresIn = 86400 };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, string? ip, string? userAgent)
    {
        var user = await _users.GetByEmailAsync(req.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLogin = DateTime.UtcNow;
        await _users.UpdateAsync(user);

        await _movements.LogAsync(user.Id, ActionType.Login, ip, userAgent);

        var token = _jwt.GenerateToken(user);

        return new AuthResponse { UserId = user.Id, Token = token, ExpiresIn = 86400 };
    }

    public async Task LogoutAsync(Guid userId)
    {
        await _movements.LogAsync(userId, ActionType.Logout, null, null);
    }
}
