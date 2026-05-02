namespace NexaCV.Api.Services;

/// <summary>A job title suggestion with a relevance score from 1 (low) to 10 (high).</summary>
public record AiJobTitleSuggestion(string Title, int Score);

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

public interface IAiService
{
    Task<AiGenerationResult> GenerateAsync(string rawDataJson);
    Task<AiRegenerationResult> RegenerateAsync(AiRegenerateContext context);
}
