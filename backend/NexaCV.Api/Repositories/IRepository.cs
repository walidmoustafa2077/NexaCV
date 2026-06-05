namespace NexaCV.Api.Repositories;

/// <summary>
/// Full CRUD contract. Extends <see cref="IWriteRepository{T}"/> with read operations.
/// Use <see cref="IWriteRepository{T}"/> directly for append-only repositories.
/// </summary>
public interface IRepository<T> : IWriteRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
}
