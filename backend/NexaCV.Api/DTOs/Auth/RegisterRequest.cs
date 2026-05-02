using FluentValidation;

namespace NexaCV.Api.DTOs.Auth;

/// <summary>Request body to create a new user account.</summary>
public class RegisterRequest
{
    /// <summary>User's first name. Max 50 characters.</summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>User's last name. Max 50 characters.</summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Unique username. Max 50 characters.</summary>
    /// <example>johndoe</example>
    public string Username { get; set; } = string.Empty;

    /// <summary>Unique email address. Max 150 characters.</summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Password — minimum 8 characters, must include at least one special character.</summary>
    /// <example>P@ssw0rd!</example>
    public string Password { get; set; } = string.Empty;

    /// <summary>Optional date of birth.</summary>
    /// <example>1995-06-15</example>
    public DateOnly? DateOfBirth { get; set; }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150)
            .Matches(@"^\S+@\S+\.\S+$").WithMessage("'Email' is not a valid email address.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
