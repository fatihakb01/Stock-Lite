using Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class InventoryPolicyHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal async Task<ProductInventoryPolicy> CreateInventoryPolicy(
            Product product,
            int safetyStock = 10,
            int leadTimeDays = 7,
            int minimumOrderQuantity = 10,
            int orderIncrementQuantity = 10,
            int stockAdjustment = 0,
            bool saveChanges = true
        )
        {
            var policy = CreateInventoryPolicyStatic(
                product: product,
                safetyStock: safetyStock,
                leadTimeDays: leadTimeDays,
                minimumOrderQuantity: minimumOrderQuantity,
                orderIncrementQuantity: orderIncrementQuantity,
                stockAdjustment: stockAdjustment
            );

            await _fixture.Context.ProductInventoryPolicies.AddAsync(policy);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return policy;
        }

        internal static ProductInventoryPolicy CreateInventoryPolicyStatic(
            Product product,
            int safetyStock = 10,
            int leadTimeDays = 7,
            int minimumOrderQuantity = 10,
            int orderIncrementQuantity = 10,
            int stockAdjustment = 0
        )
        {
            return new ProductInventoryPolicy
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

        internal async Task<ProductInventoryPolicy?> GetInventoryPolicy(
            Guid companyId,
            string sku)
        {
            return await _fixture.Context.ProductInventoryPolicies
                .AsNoTracking()
                .Include(x => x.Product)
                .SingleOrDefaultAsync(x =>
                    x.Product.CompanyId == companyId &&
                    x.Product.SKU == sku);
        }
    }
}