using Domain.Entities.Derived;

namespace Domain.Interfaces.Repositories;

public interface IStockLevelRepository : IRepository<StockLevel>
{
    Task InsertStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task UpdateStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);
    Task DeleteStockLevelsAsync(Guid companyId, IEnumerable<Guid>? productIds = null);

    Task<StockLevel?> GetByProductAsync(Guid productId);
}
