using NexaCV.Api.Data;
using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public class DownloadRepository : EfRepository<Download>, IDownloadRepository
{
    public DownloadRepository(AppDbContext db) : base(db) { }
}
