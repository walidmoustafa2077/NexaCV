using FluentValidation;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Request body for renaming a resume.</summary>
public class RenameResumeRequest
{
    /// <summary>New display name for the resume (1–100 characters).</summary>
    /// <example>Front-end Developer Resume</example>
    public string Name { get; set; } = string.Empty;
}

public class RenameResumeRequestValidator : AbstractValidator<RenameResumeRequest>
{
    public RenameResumeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
    }
}
