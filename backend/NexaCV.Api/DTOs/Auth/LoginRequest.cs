using FluentValidation;

namespace NexaCV.Api.DTOs.Auth;

/// <summary>Credentials used to authenticate an existing user.</summary>
public class LoginRequest
{
    /// <summary>Registered email address.</summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Account password.</summary>
    /// <example>P@ssw0rd!</example>
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
