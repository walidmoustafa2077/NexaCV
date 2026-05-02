using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class TransactionRepository : EfRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext db) : base(db) { }

    public async Task<Transaction?> GetByResumeIdAsync(Guid resumeId)
        => await _db.Transactions.FirstOrDefaultAsync(t => t.ResumeId == resumeId);

    public async Task<Transaction?> GetByGatewayRefIdAsync(string gatewayRefId)
        => await _db.Transactions.FirstOrDefaultAsync(t => t.GatewayRefId == gatewayRefId);
}
