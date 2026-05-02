using NexaCV.Api.Data;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class UserMovementRepository : EfRepository<UserMovement>, IUserMovementRepository
{
    public UserMovementRepository(AppDbContext db) : base(db) { }

    public async Task LogAsync(Guid userId, ActionType actionType, string? ip, string? userAgent)
    {
        var movement = new UserMovement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionType = actionType,
            IpAddress = ip,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await AddAsync(movement);
    }
}
