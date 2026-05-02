using System.Text.Json;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Result of a single AI section regeneration call.</summary>
public class RegenerateResponse
{
    /// <summary>The section that was regenerated.</summary>
    /// <example>SUMMARY</example>
    public string SectionIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// New content produced by the AI for this section.
    /// May be a JSON string, array, or object depending on the section type.
    /// </summary>
    public JsonElement UpdatedContent { get; set; }

    /// <summary>Total number of regenerations used for this section so far (including this call). Max is 3.</summary>
    /// <example>1</example>
    public int RegenCountUsed { get; set; }

    /// <summary>Remaining regenerations available for this section. Equals <c>3 - RegenCountUsed</c>.</summary>
    /// <example>2</example>
    public int RegenCountRemaining { get; set; }

    /// <summary>Cost added to the resume total for this regeneration, in US Dollars (always $0.25 USD).</summary>
    /// <example>0.25</example>
    public decimal AddedCostUsd { get; set; }

    /// <summary>Whether a real AI model produced the content. <c>false</c> while stub is active.</summary>
    /// <example>false</example>
    public bool AiAvailable { get; set; }
}
