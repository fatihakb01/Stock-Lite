using Domain.Entities.Core;
using Domain.Entities.Derived;

namespace Test.Helper
{
    internal static class StockLevelHelper
    {
        internal static StockLevel CreateStockLevel(
            Product product,
            int quantityOnHand = 10,
            int quantityOnOrder = 0,
            int quantityCommitted = 0,
            int netStockPosition = 10,
            int forecastQuantity = 10,
            int reorderPoint = 1,
            bool needsReorder = false,
            int recommendedOrderQuantity = 0
        ) => new StockLevel
        {
            Product = product,
            ProductId = product.Id,
            QuantityOnHand = quantityOnHand,
            QuantityOnOrder = quantityOnOrder,
            QuantityCommitted = quantityCommitted,
            NetStockPosition = netStockPosition,
            ForecastQuantity = forecastQuantity,
            ReorderPoint = reorderPoint,
            NeedsReorder = needsReorder,
            RecommendedOrderQuantity = recommendedOrderQuantity,
            LastCalculatedAt = DateTime.UtcNow
        };
    }
}
