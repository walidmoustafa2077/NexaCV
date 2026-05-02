using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface ITemplateRepository : IRepository<Template>
{
    Task<Template?> GetByIntIdAsync(int id);
    Task<List<Template>> GetActiveAsync(string? industryCategory = null);
}
