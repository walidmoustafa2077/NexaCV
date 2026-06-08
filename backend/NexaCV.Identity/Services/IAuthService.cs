using NexaCV.Identity.DTOs;

namespace NexaCV.Identity.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent = null);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent = null);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent = null);
    Task RevokeTokenAsync(string refreshToken, string? ipAddress, string? userAgent = null);
}
