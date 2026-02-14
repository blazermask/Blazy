using System.Linq.Expressions;

namespace Blazy.Repository.Interfaces;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets all entities with related data
    /// </summary>
    Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties);

    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an entity by ID with related data
    /// </summary>
    Task<T?> GetByIdIncludingAsync(int id, params Expression<Func<T, object>>[] includeProperties);

    /// <summary>
    /// Finds entities matching a condition
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Finds entities with pagination
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> FindPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int pageIndex,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Checks if any entity matches the condition
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts entities matching the condition
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}