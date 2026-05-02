using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface IRegenerationRepository : IRepository<Regeneration>
{
    Task<int> CountBySectionAsync(Guid resumeId, string sectionIdentifier);
    /// <summary>Returns the total regeneration cost (in USD) for a resume across all sections.</summary>
    Task<decimal> GetUsdCostSumAsync(Guid resumeId);
}
