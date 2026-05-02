using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class TemplateRepository : EfRepository<Template>, ITemplateRepository
{
    public TemplateRepository(AppDbContext db) : base(db) { }

    public async Task<List<Template>> GetActiveAsync(string? industryCategory = null)
    {
        var query = _db.Templates.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(industryCategory))
            query = query.Where(t => t.IndustryCategory == industryCategory);

        return await query.ToListAsync();
    }

    public async Task<Template?> GetByIntIdAsync(int id)
        => await _db.Templates.FindAsync(id);
}
