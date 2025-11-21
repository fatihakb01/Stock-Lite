using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IUniquenessChecker<TEntity> where TEntity : class
{
    Task<bool> IsUniqueAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );
}
