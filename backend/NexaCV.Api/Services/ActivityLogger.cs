using NexaCV.Api.Data;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;

namespace NexaCV.Api.Services;

/// <summary>
/// Persists <see cref="ActivityLog"/> records to the database.
/// Falls back to the current HTTP context for IP address and User-Agent when not explicitly provided.
/// </summary>
public class ActivityLogger : IActivityLogger
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityLogger(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(Guid userId, BusinessActionType actionType, string? ipAddress = null, string? userAgent = null)
    {
        ipAddress ??= _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        userAgent ??= _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        _db.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionType = actionType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}