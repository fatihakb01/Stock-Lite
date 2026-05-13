using Domain.Entities.Core;
using Domain.Entities.Derived;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class StockLevelHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal static StockLevel CreateStockLevelStatic(
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
        internal async Task<StockLevel> CreateStockLevel(
            Product product,
            int quantityOnHand = 10,
            int quantityOnOrder = 0,
            int quantityCommitted = 0,
            int netStockPosition = 10,
            int forecastQuantity = 10,
            int reorderPoint = 1,
            bool needsReorder = false,
            int recommendedOrderQuantity = 0
        )
        {
            var stockLevel = CreateStockLevelStatic(
                product: product,
                quantityOnHand: quantityOnHand,
                quantityOnOrder: quantityOnOrder,
                quantityCommitted: quantityCommitted,
                netStockPosition: netStockPosition,
                forecastQuantity: forecastQuantity,
                reorderPoint: reorderPoint,
                needsReorder: needsReorder,
                recommendedOrderQuantity: recommendedOrderQuantity
            );

            await _fixture.Context.StockLevels.AddAsync(stockLevel);
            await _fixture.Context.SaveChangesAsync();

            return stockLevel;
        }
    }
}
