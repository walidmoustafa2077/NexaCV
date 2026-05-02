using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class ResumeHistoryRepository : EfRepository<ResumeHistory>, IResumeHistoryRepository
{
    public ResumeHistoryRepository(AppDbContext db) : base(db) { }

    public async Task<List<ResumeHistory>> GetByResumeIdAsync(Guid resumeId)
        => await _db.ResumeHistories
            .Where(h => h.ResumeId == resumeId)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

    public async Task PruneAsync(Guid resumeId, int maxSnapshots = 10)
    {
        var toDelete = await _db.ResumeHistories
            .Where(h => h.ResumeId == resumeId)
            .OrderByDescending(h => h.CreatedAt)
            .Skip(maxSnapshots)
            .ToListAsync();

        if (toDelete.Count == 0) return;

        _db.ResumeHistories.RemoveRange(toDelete);
        await _db.SaveChangesAsync();
    }
}
