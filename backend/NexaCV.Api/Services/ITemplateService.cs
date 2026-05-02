using NexaCV.Api.DTOs.Templates;

namespace NexaCV.Api.Services;

public interface ITemplateService
{
    Task<List<TemplateDto>> GetAllAsync(string? industryCategory);
    Task<TemplateDto> GetByIdAsync(int id);
}
