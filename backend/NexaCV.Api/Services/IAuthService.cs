using NexaCV.Api.DTOs.Auth;

namespace NexaCV.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req, string? ip, string? userAgent);
    Task<AuthResponse> LoginAsync(LoginRequest req, string? ip, string? userAgent);
    Task LogoutAsync(Guid userId);
}
