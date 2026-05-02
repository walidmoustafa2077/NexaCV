using Microsoft.EntityFrameworkCore;
using NexaCV.Api.Data;

namespace NexaCV.Api.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;

    public EfRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<T?> GetByIdAsync(Guid id)
        => await _db.Set<T>().FindAsync(id);

    public async Task<List<T>> GetAllAsync()
        => await _db.Set<T>().ToListAsync();

    public async Task AddAsync(T entity)
    {
        await _db.Set<T>().AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync();
    }
}
