using NexaCV.Api.DTOs.Users;

namespace NexaCV.Api.Services;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserRequest req);
}
