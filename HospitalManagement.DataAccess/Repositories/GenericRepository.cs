using System.Linq.Expressions;
using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.DataAccess.Repositories;

/// <summary>
/// Generic EF Core repository implementation. Shared by all entity types via UnitOfWork.
/// </summary>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, ct);

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    /// <inheritdoc/>
    public async Task<int> CountAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.CountAsync(predicate, ct);

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    /// <inheritdoc/>
    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    /// <inheritdoc/>
    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        Update(entity);
    }

    /// <inheritdoc/>
    public IQueryable<T> Query()
        => _dbSet.AsQueryable();
}
