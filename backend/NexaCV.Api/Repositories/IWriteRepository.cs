namespace NexaCV.Api.Repositories;

/// <summary>
/// Minimal write-only contract for repositories whose callers never need to query by ID or list all records.
/// Repositories such as <see cref="IDownloadRepository"/> and <see cref="IUserMovementRepository"/> only
/// ever append rows, so forcing them to inherit the full <see cref="IRepository{T}"/> read surface violates ISP.
/// </summary>
public interface IWriteRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
