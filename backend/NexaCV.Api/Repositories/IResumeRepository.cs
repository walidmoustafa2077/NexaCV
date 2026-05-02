using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface IResumeRepository : IRepository<Resume>
{
    Task<List<Resume>> GetByUserIdAsync(Guid userId);
    Task<Resume?> GetWithTemplateAsync(Guid resumeId);
}
