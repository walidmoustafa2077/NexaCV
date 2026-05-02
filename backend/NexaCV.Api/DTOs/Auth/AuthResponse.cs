namespace NexaCV.Api.DTOs.Auth;

/// <summary>Returned after a successful register or login call.</summary>
public class AuthResponse
{
    /// <summary>The authenticated user's unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid UserId { get; set; }

    /// <summary>Signed HS256 JWT. Pass as <c>Authorization: Bearer &lt;token&gt;</c> on every protected request.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Token lifetime in seconds (86 400 = 24 h).</summary>
    /// <example>86400</example>
    public int ExpiresIn { get; set; } = 86400;
}
