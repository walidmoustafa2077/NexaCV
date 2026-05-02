using NexaCV.Api.Enums;

namespace NexaCV.Api.Models;

public class UserMovement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ActionType ActionType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
