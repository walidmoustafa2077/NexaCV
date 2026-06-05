using System.Text.Json.Serialization;

namespace NexaCV.Api.Services;

/// <summary>A job title suggestion with a relevance score from 1 (low) to 10 (high).</summary>
public record AiJobTitleSuggestion(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("score")] int Score);

public record AiGenerationResult(
    string FinalDataJson,
    bool AiAvailable,
    IReadOnlyList<AiJobTitleSuggestion>? JobTitleSuggestions = null,
    IReadOnlyList<string>? SkillSuggestions = null);

/// <summary>
/// Structured context passed to the AI for a context-aware section regeneration.
/// Carries both the user's intent and the surrounding resume state so the AI
/// can produce output that is coherent with the full document.
/// </summary>
public record AiRegenerateContext(
    string SectionIdentifier,
    string UserPrompt,
    string? TargetFormat,
    string? NewTitleSuggestion,
    string CurrentSectionContent,
    string? ResumeTitle,
    string? Skills,
    string? CurrentDescriptionFormat);

public record AiRegenerationResult(string UpdatedContent, bool AiAvailable);

/// <summary>
/// Responsible solely for generating a complete resume from raw wizard data.
/// Segregated from <see cref="IResumeSectionRegenerationService"/> so that callers
/// that only create resumes (e.g. <c>ResumeService</c>) do not depend on the
/// regeneration contract (ISP).
/// </summary>
public interface IResumeGenerationService
{
    Task<AiGenerationResult> GenerateAsync(string rawDataJson);
}

/// <summary>
/// Responsible solely for regenerating a single section of an existing resume.
/// Segregated from <see cref="IResumeGenerationService"/> so that callers that only
/// regenerate sections (e.g. <c>RegenerationService</c>) do not depend on the
/// full generation contract (ISP).
/// </summary>
public interface IResumeSectionRegenerationService
{
    Task<AiRegenerationResult> RegenerateAsync(AiRegenerateContext context);
}
