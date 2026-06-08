using NexaCV.Api.Enums;

namespace NexaCV.Api.Models;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public BusinessActionType ActionType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    public NexaCvUserProfile User { get; set; } = null!;
}
