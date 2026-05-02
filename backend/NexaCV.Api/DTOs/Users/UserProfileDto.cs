namespace NexaCV.Api.DTOs.Users;

/// <summary>Public profile of the currently authenticated user. Never exposes the password hash.</summary>
public class UserProfileDto
{
    /// <summary>User's unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>First name.</summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name.</summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Unique username.</summary>
    /// <example>johndoe</example>
    public string Username { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>UTC timestamp of account creation.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the most recent successful login. Null if the user has never logged in after registration.</summary>
    public DateTime? LastLogin { get; set; }
}
