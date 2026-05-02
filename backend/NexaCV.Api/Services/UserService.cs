using NexaCV.Api.DTOs.Users;
using NexaCV.Api.Enums;
using NexaCV.Api.Extensions;
using NexaCV.Api.Repositories;

namespace NexaCV.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUserMovementRepository _movements;

    public UserService(IUserRepository users, IUserMovementRepository movements)
    {
        _users = users;
        _movements = movements;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return user.ToProfileDto();
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserRequest req)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (req.FirstName is not null) user.FirstName = req.FirstName;
        if (req.LastName is not null) user.LastName = req.LastName;
        if (req.Username is not null) user.Username = req.Username;

        if (req.Password is not null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            await _movements.LogAsync(userId, ActionType.PasswordUpdated, null, null);
        }

        await _users.UpdateAsync(user);

        return user.ToProfileDto();
    }
}
