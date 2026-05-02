using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class RegenerationRepository : EfRepository<Regeneration>, IRegenerationRepository
{
    public RegenerationRepository(AppDbContext db) : base(db) { }

    public async Task<int> CountBySectionAsync(Guid resumeId, string sectionIdentifier)
        => await _db.Regenerations
            .CountAsync(r => r.ResumeId == resumeId && r.SectionIdentifier == sectionIdentifier);

    public async Task<decimal> GetUsdCostSumAsync(Guid resumeId)
    {
        var costs = await _db.Regenerations
            .Where(r => r.ResumeId == resumeId)
            .Select(r => r.CostUsd)
            .ToListAsync();
        return costs.Sum();
    }
}
