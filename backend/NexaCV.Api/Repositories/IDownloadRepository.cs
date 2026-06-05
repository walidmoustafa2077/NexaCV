using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

/// <summary>
/// Append-only repository for download audit records.
/// Extends <see cref="IWriteRepository{T}"/> only — callers never query downloads by ID or list all rows.
/// </summary>
public interface IDownloadRepository : IWriteRepository<Download>
{
}
