using System.ComponentModel.DataAnnotations;

namespace NexaCV.Identity.DTOs;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(3), MaxLength(50), RegularExpression(@"^[a-zA-Z0-9._-]+$")]
    string Username,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    DateOnly? DateOfBirth
);

public sealed record LoginRequest(
    [Required, MinLength(3), MaxLength(256)] string EmailOrUsername,
    [Required] string Password
);

/// <summary>Used to exchange a Refresh Token for a new Access + Refresh Token pair.</summary>
public sealed record TokenRequest(
    [Required] string RefreshToken
);

/// <summary>Used to explicitly revoke a Refresh Token (e.g., on logout).</summary>
public sealed record RevokeTokenRequest(
    [Required] string RefreshToken
);

public sealed record AuthResponse(
    Guid UserId,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);
