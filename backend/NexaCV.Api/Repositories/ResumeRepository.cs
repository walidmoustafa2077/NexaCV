using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class ResumeRepository : EfRepository<Resume>, IResumeRepository
{
    public ResumeRepository(AppDbContext db) : base(db) { }

    public async Task<List<Resume>> GetByUserIdAsync(Guid userId)
        => await _db.Resumes
            .Include(r => r.Template)
            .Include(r => r.Downloads)
            .Where(r => r.UserId == userId)
            .ToListAsync();

    public async Task<Resume?> GetWithTemplateAsync(Guid resumeId)
        => await _db.Resumes
            .Include(r => r.Template)
            .FirstOrDefaultAsync(r => r.Id == resumeId);
}
