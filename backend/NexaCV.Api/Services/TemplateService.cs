using NexaCV.Api.DTOs.Templates;
using NexaCV.Api.Extensions;
using NexaCV.Api.Repositories;

namespace NexaCV.Api.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templates;

    public TemplateService(ITemplateRepository templates)
    {
        _templates = templates;
    }

    public async Task<List<TemplateDto>> GetAllAsync(string? industryCategory)
    {
        var templates = await _templates.GetActiveAsync(industryCategory);
        return templates.Select(t => t.ToDto()).ToList();
    }

    public async Task<TemplateDto> GetByIdAsync(int id)
    {
        var template = await _templates.GetByIntIdAsync(id)
            ?? throw new KeyNotFoundException($"Template with ID {id} not found.");

        return template.ToDto();
    }
}
