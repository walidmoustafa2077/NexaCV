namespace NexaCV.Api.DTOs.Users;

/// <summary>Partial update for the current user's profile. Only non-null fields are applied.</summary>
public class UpdateUserRequest
{
    /// <summary>New first name. Leave null to keep unchanged.</summary>
    /// <example>Jane</example>
    public string? FirstName { get; set; }

    /// <summary>New last name. Leave null to keep unchanged.</summary>
    /// <example>Smith</example>
    public string? LastName { get; set; }

    /// <summary>New username. Must be unique. Leave null to keep unchanged.</summary>
    /// <example>janesmith</example>
    public string? Username { get; set; }

    /// <summary>New password. If provided, it will be re-hashed and a PASSWORD_UPDATED movement will be logged. Leave null to keep unchanged.</summary>
    /// <example>N3wP@ss!</example>
    public string? Password { get; set; }
}
