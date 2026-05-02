using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface IUserMovementRepository : IRepository<UserMovement>
{
    Task LogAsync(Guid userId, ActionType actionType, string? ip, string? userAgent);
}
