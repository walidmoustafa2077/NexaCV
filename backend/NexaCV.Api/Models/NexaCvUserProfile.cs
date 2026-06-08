namespace NexaCV.Api.Models;

public class NexaCvUserProfile
{
    public Guid UserId { get; set; }  // PK + FK to shared Users table

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public bool IsPremiumUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }

    // Navigation — parent for business entities
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
