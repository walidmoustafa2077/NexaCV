using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

/// <summary>
/// Append-only repository for user audit movements.
/// Extends <see cref="IWriteRepository{T}"/> only — callers never query movements by ID or list all rows.
/// </summary>
public interface IUserMovementRepository : IWriteRepository<UserMovement>
{
    Task LogAsync(Guid userId, ActionType actionType, string? ip, string? userAgent);
}
