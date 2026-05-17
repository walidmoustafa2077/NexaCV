namespace NexaCV.Api.DTOs.Templates;

/// <summary>
/// Describes what sections, layouts, and limits a specific template supports.
/// Used by the frontend wizard to enforce template-specific constraints at data-entry time.
/// </summary>
public class TemplateCapabilities
{
    /// <summary>Summary types this template renders correctly. Values: SUMMARY | OBJECTIVE</summary>
    public List<string> SupportedSummaryTypes { get; set; } = ["SUMMARY", "OBJECTIVE"];

    /// <summary>Description formats this template is designed for. Values: BULLETED | PARAGRAPH</summary>
    public List<string> SupportedDescriptionFormats { get; set; } = ["BULLETED", "PARAGRAPH"];

    /// <summary>Skills layout modes this template supports. Values: FLAT | MIXED | CATEGORIZED</summary>
    public List<string> SupportedSkillsLayouts { get; set; } = ["FLAT", "MIXED", "CATEGORIZED"];

    /// <summary>Maximum number of experience entries the template layout accommodates cleanly.</summary>
    public int MaxExperienceItems { get; set; } = 10;

    /// <summary>Whether the template includes a dedicated hobbies section.</summary>
    public bool HasHobbySection { get; set; } = false;

    /// <summary>Whether the template includes a projects section.</summary>
    public bool HasProjectSection { get; set; } = true;

    /// <summary>Whether the template includes a languages section.</summary>
    public bool HasLanguageSection { get; set; } = true;
}
