namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Lightweight resume representation used in list responses.</summary>
public class ResumeSummaryDto
{
    /// <summary>Resume unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>Current status of the resume. One of: <c>DRAFT</c>, <c>COMPLETED</c>, <c>PAID</c>.</summary>
    /// <example>COMPLETED</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>Name of the template used.</summary>
    /// <example>Modern Minimalist</example>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the resume was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last update.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>User-defined resume name. Null when not yet named.</summary>
    public string? Name { get; set; }

    /// <summary>Number of times this resume has been downloaded.</summary>
    public int DownloadCount { get; set; }
}
