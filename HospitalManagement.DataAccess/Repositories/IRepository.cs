using System.Linq.Expressions;
using HospitalManagement.DataAccess.Models;

namespace HospitalManagement.DataAccess.Repositories;

/// <summary>
/// Generic repository contract. Provides a consistent async API for all entity types.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Find a single entity by its primary key.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Return all non-deleted entities (no tracking).</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Return entities matching the predicate (no tracking).</summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Return the first entity matching the predicate or null.</summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Return true if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Return the count of entities matching the predicate.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Add a new entity to the change tracker.</summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Mark an entity as modified in the change tracker.</summary>
    void Update(T entity);

    /// <summary>Soft-delete an entity (sets IsDeleted = true).</summary>
    void Delete(T entity);

    /// <summary>Expose the IQueryable for complex queries (eager loading, projections, etc.).</summary>
    IQueryable<T> Query();
}
