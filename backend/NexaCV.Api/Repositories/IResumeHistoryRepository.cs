using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface IResumeHistoryRepository : IRepository<ResumeHistory>
{
    Task<List<ResumeHistory>> GetByResumeIdAsync(Guid resumeId);

    /// <summary>
    /// Deletes the oldest snapshots for a resume, keeping only the most recent
    /// <paramref name="maxSnapshots"/> entries. Call after every AddAsync.
    /// </summary>
    Task PruneAsync(Guid resumeId, int maxSnapshots = 10);
}
