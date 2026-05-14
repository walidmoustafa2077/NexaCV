namespace NexaCV.Api.Services;

/// <summary>
/// Renders a template's HTML content by replacing placeholder tokens with data from a resume's FinalData JSON.
/// </summary>
public interface ITemplateRendererService
{
    /// <summary>
    /// Returns a fully rendered HTML string ready for display or PDF conversion.
    /// </summary>
    /// <param name="htmlTemplate">Raw HTML with {{TOKEN}} and <!--REPEAT:SECTION-->…<!--/REPEAT:SECTION--> markers.</param>
    /// <param name="finalDataJson">The resume's FinalData JSON (same shape as RawResumeData).</param>
    string Render(string htmlTemplate, string finalDataJson);
}
