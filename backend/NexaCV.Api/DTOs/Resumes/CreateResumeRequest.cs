using FluentValidation;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Payload to create a new resume from the wizard form data.</summary>
public class CreateResumeRequest
{
    /// <summary>ID of the template to use. Must match an active template from <c>GET /api/templates</c>.</summary>
    /// <example>1</example>
    public int TemplateId { get; set; }

    /// <summary>
    /// Structured form data from the resume wizard.
    /// Contains settings and content (personal info, experience, education, etc.).
    /// </summary>
    public RawResumeData RawData { get; set; } = new();
}

public class CreateResumeRequestValidator : AbstractValidator<CreateResumeRequest>
{
    public CreateResumeRequestValidator()
    {
        RuleFor(x => x.TemplateId).GreaterThan(0).WithMessage("TemplateId must be a valid positive integer.");

        // ── Personal info ─────────────────────────────────────────────────────
        RuleFor(x => x.RawData.Content.Personal.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.RawData.Content.Personal.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.RawData.Content.Personal.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.RawData.Content.Personal.Phone)
            .NotEmpty().WithMessage("Phone number is required.");

        RuleFor(x => x.RawData.Content.Personal.Location)
            .NotEmpty().WithMessage("Location is required.");

        // ── Education ─────────────────────────────────────────────────────────
        RuleForEach(x => x.RawData.Content.Education).ChildRules(edu =>
        {
            edu.RuleFor(e => e.Institution)
                .NotEmpty().WithMessage("Institution name is required.");

            edu.RuleFor(e => e.Degree)
                .NotEmpty().WithMessage("Degree / Qualification is required.");

            edu.RuleFor(e => e.EndDate)
                .NotEmpty().WithMessage("Graduation date is required.");
        });

        // ── Experience ────────────────────────────────────────────────────────
        RuleFor(x => x.RawData.Content.Experience)
            .NotEmpty().WithMessage("At least one experience entry is required.");

        RuleForEach(x => x.RawData.Content.Experience).ChildRules(exp =>
        {
            exp.RuleFor(e => e.Title)
                .NotEmpty().WithMessage("Job title is required.");

            exp.RuleFor(e => e.Company)
                .NotEmpty().WithMessage("Company is required.");

            exp.RuleFor(e => e.Description)
                .NotEmpty().WithMessage("Description & key achievements is required.");
        });
    }
}
