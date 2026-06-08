using NexaCV.Api.Data;
using NexaCV.Api.DTOs.Profile;
using NexaCV.Api.Enums;
using NexaCV.Api.Extensions;
using NexaCV.Api.Models;

namespace NexaCV.Api.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    private readonly IActivityLogger _activityLogger;

    public ProfileService(AppDbContext db, IActivityLogger activityLogger)
    {
        _db = db;
        _activityLogger = activityLogger;
    }

    public async Task<ProfileDto?> GetProfileAsync(Guid userId)
    {
        var profile = await _db.Profiles.FindAsync(userId);
        return profile?.ToProfileDto();
    }

    public async Task<ProfileDto> CreateProfileAsync(Guid userId, CreateProfileRequest req)
    {
        var profile = new NexaCvUserProfile
        {
            UserId = userId,
            Bio = req.Bio,
            CreatedAt = DateTime.UtcNow
        };

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();

        await _activityLogger.LogAsync(userId, BusinessActionType.ProfileUpdated);

        return profile.ToProfileDto();
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
    {
        var profile = await _db.Profiles.FindAsync(userId)
            ?? throw new KeyNotFoundException("Profile not found.");

        if (req.FirstName is not null) profile.FirstName = req.FirstName;
        if (req.LastName is not null) profile.LastName = req.LastName;
        if (req.Username is not null) profile.Username = req.Username;
        if (req.Bio is not null) profile.Bio = req.Bio;

        await _db.SaveChangesAsync();

        await _activityLogger.LogAsync(userId, BusinessActionType.ProfileUpdated);

        return profile.ToProfileDto();
    }
}
