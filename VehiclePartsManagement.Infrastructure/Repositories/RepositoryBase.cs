using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

/// <summary>
/// Default EF Core implementation of <see cref="IRepositoryBase{T}"/>.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class RepositoryBase<T>(AppDbContext context) : IRepositoryBase<T> where T : class
{
    /// <summary>
    /// Backing database context.
    /// </summary>
    protected AppDbContext Context { get; } = context;

    /// <inheritdoc />
    public IQueryable<T> FindAll(bool trackChanges = false) =>
        // Read queries should not track by default to reduce memory and change tracker overhead.
        !trackChanges
            ? Context.Set<T>().AsNoTracking()
            : Context.Set<T>();


    /// <inheritdoc />
    public async Task<List<T>> FindAllAsync(bool trackChanges = false) =>
        !trackChanges
            ? await Context.Set<T>().AsNoTracking().ToListAsync()
            : await Context.Set<T>().ToListAsync();


    /// <inheritdoc />
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
        !trackChanges
            ? Context.Set<T>().Where(expression).AsNoTracking()
            : Context.Set<T>().Where(expression);


    /// <inheritdoc />
    public T? GetById(int id) => Context.Set<T>().Find(id);


    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id) => await Context.Set<T>().FindAsync(id);


    /// <inheritdoc />
    public void Create(T entity) => Context.Set<T>().Add(entity);


    /// <inheritdoc />
    public void Update(T entity) => Context.Set<T>().Update(entity);


    /// <inheritdoc />
    public void Delete(T entity) => Context.Set<T>().Remove(entity);


    /// <inheritdoc />
    public void AddRange(IEnumerable<T> entities) => Context.Set<T>().AddRange(entities);


    /// <inheritdoc />
    public void DeleteRange(IEnumerable<T> entities) => Context.Set<T>().RemoveRange(entities);


    /// <inheritdoc />
    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
