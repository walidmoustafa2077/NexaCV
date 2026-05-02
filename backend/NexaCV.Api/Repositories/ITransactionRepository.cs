using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByResumeIdAsync(Guid resumeId);
    Task<Transaction?> GetByGatewayRefIdAsync(string gatewayRefId);
}
