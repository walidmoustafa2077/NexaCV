using NexaCV.Api.Enums;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Top-level shape of the <c>rawData</c> wizard payload.</summary>
public class RawResumeData
{
    public RawResumeSettings Settings { get; set; } = new();
    public RawResumeContent Content { get; set; } = new();
}

public class RawResumeSettings
{
    public SummaryType? SummaryType { get; set; }
    public DescriptionFormat? DescriptionFormat { get; set; }
}

public class RawResumeContent
{
    public PersonalInfo Personal { get; set; } = new();
    public string? Summary { get; set; }
    public List<ExperienceEntry> Experience { get; set; } = [];
    public List<EducationEntry> Education { get; set; } = [];
    public List<CourseEntry>? Courses { get; set; }
    public List<string>? Skills { get; set; }
}

public class PersonalInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? SiteUrl { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ExperienceEntry
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class EducationEntry
{
    public string? Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

public class CourseEntry
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? Date { get; set; }
}
