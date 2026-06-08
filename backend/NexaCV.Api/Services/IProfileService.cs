using NexaCV.Api.DTOs.Profile;

namespace NexaCV.Api.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId);
    Task<ProfileDto> CreateProfileAsync(Guid userId, CreateProfileRequest req);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest req);
}
