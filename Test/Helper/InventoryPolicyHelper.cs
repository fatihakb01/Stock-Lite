using Domain.Entities.Core;

namespace Test.Helper
{
    internal static class InventoryPolicyHelper
    {
        internal static ProductInventoryPolicy CreateInventoryPolicy(
            Product product,
            int safetyStock = 10,
            int leadTimeDays = 7,
            int minimumOrderQuantity = 10,
            int orderIncrementQuantity = 10,
            int stockAdjustment = 0
        ) => new ProductInventoryPolicy
        {
            ProductId = product.Id,
            Product = product,
            SafetyStock = safetyStock,
            LeadTimeDays = leadTimeDays,
            MinimumOrderQuantity = minimumOrderQuantity,
            OrderIncrementQuantity = orderIncrementQuantity,
            StockAdjustment = stockAdjustment
        };
    }
}