using FluentValidation;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Request to regenerate a single section of a resume's <c>FinalData.content</c> using AI.</summary>
public class RegenerateRequest
{
    /// <summary>
    /// Key inside <c>FinalData.content</c> that identifies the section to regenerate.
    /// Must exactly match the property name in the content object (e.g. <c>summary</c>, <c>experience</c>, <c>skills</c>).
    /// </summary>
    /// <example>summary</example>
    public string SectionIdentifier { get; set; } = string.Empty;

    /// <summary>Free-text instruction for the AI. Describes the desired tone, focus, or content changes.</summary>
    /// <example>Make it more concise and achievement-focused</example>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Request a structural format change for this section.
    /// For experience/summary sections: <c>BULLET</c> or <c>PARAGRAPH</c>.
    /// For the skills section: <c>GRID</c> or <c>LIST</c>.
    /// When provided, the corresponding key in <c>FinalData.settings</c> is updated.
    /// </summary>
    /// <example>PARAGRAPH</example>
    public string? TargetFormat { get; set; }

    /// <summary>
    /// Optional. A new job title suggestion. When provided, the AI aligns the regenerated content
    /// to match responsibilities and language appropriate for this title.
    /// </summary>
    /// <example>Senior Cloud Architect</example>
    public string? NewTitleSuggestion { get; set; }
}

public class RegenerateRequestValidator : AbstractValidator<RegenerateRequest>
{
    public RegenerateRequestValidator()
    {
        RuleFor(x => x.SectionIdentifier).NotEmpty().WithMessage("SectionIdentifier is required.");
        RuleFor(x => x.UserPrompt).NotEmpty().WithMessage("UserPrompt is required.");
    }
}
