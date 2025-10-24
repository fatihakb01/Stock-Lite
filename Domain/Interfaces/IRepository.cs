using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    void Remove(T entity);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    // IQueryable<T> GetQueryable();
}
