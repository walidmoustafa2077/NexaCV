using NexaCV.Api.Enums;

namespace NexaCV.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }

    public ICollection<UserMovement> UserMovements { get; set; } = new List<UserMovement>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
