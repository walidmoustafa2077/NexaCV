using NexaCV.Api.Enums;

namespace NexaCV.Api.Services;

/// <summary>
/// Abstracts audit logging of business actions.
/// Each call persists an <see cref="Models.ActivityLog"/> row.
/// IP address and User-Agent are extracted from the HTTP context if not explicitly provided.
/// </summary>
public interface IActivityLogger
{
    /// <summary>Log a business action performed by the specified user.</summary>
    Task LogAsync(Guid userId, BusinessActionType actionType, string? ipAddress = null, string? userAgent = null);
}