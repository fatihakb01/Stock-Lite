using System.Linq.Expressions;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class Repository<T>(DataContext context) 
    : IRepository<T> where T : class
{
    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);
    }
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Set<T>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Set<T>().FindAsync([id], cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<T>().ToListAsync(cancellationToken);
    }

    public void Remove(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<T, object>>[]? includes)
    {
        IQueryable<T> query = context.Set<T>().Where(predicate);

        // Apply includes if provided
        foreach (var include in includes!)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TResult>> FindSelectAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken)
    {
        return await context.Set<T>()
            .Where(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }
}
