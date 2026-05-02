using System.Text.Json;
using NexaCV.Api.Services;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Full detail view of a resume including raw and AI-processed data.</summary>
public class ResumeDetailDto
{
    /// <summary>Resume unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>Current status. One of: <c>DRAFT</c>, <c>COMPLETED</c>, <c>PAID</c>.</summary>
    /// <example>COMPLETED</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>ID of the template used.</summary>
    /// <example>1</example>
    public int TemplateId { get; set; }

    /// <summary>Name of the template used.</summary>
    /// <example>Modern Minimalist</example>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Original wizard form data JSON submitted at creation time.</summary>
    public JsonElement? RawData { get; set; }

    /// <summary>AI-enhanced resume data JSON. This is what gets rendered into the final PDF/DOCX.</summary>
    public JsonElement? FinalData { get; set; }

    /// <summary>
    /// Indicates whether a real AI model processed this resume.
    /// <c>false</c> while the stub AI is active — <c>FinalData</c> mirrors <c>RawData</c> in that case.
    /// </summary>
    /// <example>false</example>
    public bool AiAvailable { get; set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last mutation (update, regeneration, soft-delete).</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// AI-suggested job titles ranked by relevance (score 1–10).
    /// Populated only on creation. Not stored in the database.
    /// </summary>
    public IReadOnlyList<AiJobTitleSuggestion>? JobTitleSuggestions { get; set; }

    /// <summary>
    /// Up to 10 complementary skill suggestions not already in the resume.
    /// Populated only on creation. Not stored in the database.
    /// </summary>
    public IReadOnlyList<string>? SkillSuggestions { get; set; }
}
