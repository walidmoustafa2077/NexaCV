namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Request body for renaming a resume.</summary>
public class RenameResumeRequest
{
    /// <summary>New display name for the resume (1–100 characters).</summary>
    /// <example>Front-end Developer Resume</example>
    public string Name { get; set; } = string.Empty;
}
