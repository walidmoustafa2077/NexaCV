namespace NexaCV.Api.DTOs.Templates;

/// <summary>A resume template available for selection during the resume wizard.</summary>
public class TemplateDto
{
    /// <summary>Auto-incremented template identifier.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Human-readable template name.</summary>
    /// <example>Modern Minimalist</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Industry category this template is optimised for. Null means generic.</summary>
    /// <example>Corporate</example>
    public string? IndustryCategory { get; set; }

    /// <summary>Base price in US Dollars. Final price in the user's local currency is calculated at checkout.</summary>
    /// <example>3.00</example>
    public decimal BasePriceUsd { get; set; }

    /// <summary>Whether this template can be exported as a DOCX file in addition to PDF.</summary>
    /// <example>true</example>
    public bool SupportsWord { get; set; }
}
