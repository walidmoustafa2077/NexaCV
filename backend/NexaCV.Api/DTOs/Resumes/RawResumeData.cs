using NexaCV.Api.Enums;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Top-level shape of the resume wizard payload. Settings are defined by the template, not the resume.</summary>
public class RawResumeData
{
    public string SchemaVersion { get; set; } = "1.2";
    public RawResumeContent Content { get; set; } = new();
}

public class RawResumeContent
{
    public string? TargetJobTitle { get; set; }
    public PersonalInfo Personal { get; set; } = new();
    public string? Summary { get; set; }
    public List<ExperienceEntry> Experience { get; set; } = [];
    public List<EducationEntry> Education { get; set; } = [];
    public List<CourseEntry>? Courses { get; set; }
    public List<ProjectEntry>? Projects { get; set; }
    public List<SkillEntry>? Skills { get; set; }
    public List<LanguageEntry>? Languages { get; set; }
    public List<VolunteerEntry>? Volunteers { get; set; }
    public List<string>? Hobbies { get; set; }
    public List<OtherEntry>? Other { get; set; }
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
    public string? ZipCode { get; set; }
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
    public int? WordCount { get; set; }
}

public class EducationEntry
{
    public string? Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public string? Grade { get; set; }
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

public class ProjectEntry
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Description { get; set; }
    public string? Link { get; set; }
    public List<string>? Technologies { get; set; }
    public int? WordCount { get; set; }
}

public class SkillEntry
{
    public string Name { get; set; } = string.Empty;
    public SkillType? Type { get; set; }
    public string? Category { get; set; }
}

public class LanguageEntry
{
    public string Language { get; set; } = string.Empty;
    public LanguageLevel? Level { get; set; }
}

public class OtherEntry
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class VolunteerEntry
{
    public string? Id { get; set; }
    public string Organization { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Description { get; set; }
    public int? WordCount { get; set; }
}
