using NexaCV.Identity.Models;

namespace NexaCV.Identity.Services;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress);
}
