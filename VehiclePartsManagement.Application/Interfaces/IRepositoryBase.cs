using System.Linq.Expressions;

namespace VehiclePartsManagement.Application.Interfaces;

/// <summary>
/// Provides a generic repository contract for common persistence operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepositoryBase<T> where T : class
{
    /// <summary>
    /// Returns a query for all entities.
    /// </summary>
    /// <param name="trackChanges">When true, enables EF Core tracking.</param>
    IQueryable<T> FindAll(bool trackChanges = false);


    /// <summary>
    /// Returns all entities as a list.
    /// </summary>
    /// <param name="trackChanges">When true, enables EF Core tracking.</param>
    Task<List<T>> FindAllAsync(bool trackChanges = false);


    /// <summary>
    /// Returns a query filtered by a predicate.
    /// </summary>
    /// <param name="expression">Filter expression.</param>
    /// <param name="trackChanges">When true, enables EF Core tracking.</param>
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);


    /// <summary>
    /// Finds an entity by integer primary key.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    T? GetById(int id);


    /// <summary>
    /// Finds an entity by integer primary key asynchronously.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    Task<T?> GetByIdAsync(int id);


    /// <summary>
    /// Adds a new entity to the context.
    /// </summary>
    void Create(T entity);


    /// <summary>
    /// Marks an entity as modified.
    /// </summary>
    void Update(T entity);


    /// <summary>
    /// Removes an entity from the context.
    /// </summary>
    void Delete(T entity);


    /// <summary>
    /// Persists pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();


    /// <summary>
    /// Adds multiple entities to the context.
    /// </summary>
    void AddRange(IEnumerable<T> entities);


    /// <summary>
    /// Removes multiple entities from the context.
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);
}
