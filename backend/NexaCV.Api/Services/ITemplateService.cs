using NexaCV.Api.DTOs.Templates;

namespace NexaCV.Api.Services;

public interface ITemplateService
{
    Task<List<TemplateDto>> GetAllAsync(string? industryCategory);
    Task<TemplateDto> GetByIdAsync(int id);
    /// <summary>Returns the raw HTML content (with tokens) for a template, or null if not found.</summary>
    Task<string?> GetHtmlContentAsync(int id);
}
