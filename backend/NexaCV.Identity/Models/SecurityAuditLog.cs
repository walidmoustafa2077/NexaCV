namespace NexaCV.Identity.Models;

public class SecurityAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SecurityEventType Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
