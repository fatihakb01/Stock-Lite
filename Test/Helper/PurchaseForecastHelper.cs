using Domain.Entities.Core;
using Domain.Entities.Derived;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;
using Domain.Enums;

namespace Test.Helper
{
    internal class PurchaseForecastHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal static PurchaseForecast CreatePurchaseForecast(
            Product product,
            DateTime? weekStartDate = null,
            DateTime? weekEndDate = null,
            int weekIndex = 0,
            decimal forecastQuantity = 100,
            int quantityCommitted = 0,
            int quantityOnOrder = 0,
            decimal projectedStock = 15,
            bool needsReorder = false,
            int recommendedOrderQuantity = 0
        ) => new PurchaseForecast
        {
            Product = product,
            ProductId = product.Id,
            WeekStartDate = weekStartDate ?? DateHelper.GetPeriodStartDate(Period.Weekly),
            WeekEndDate = weekEndDate ?? DateHelper.GetPeriodEndDate(Period.Weekly),
            WeekIndex = weekIndex,
            ForecastQuantity = forecastQuantity,
            QuantityCommitted = quantityCommitted,
            QuantityOnOrder = quantityOnOrder,
            ProjectedStock = projectedStock,
            NeedsReorder = needsReorder,
            RecommendedOrderQuantity= recommendedOrderQuantity,
            CalculatedAt = DateTime.UtcNow
        };

        internal async Task<PurchaseForecast> GetPurchaseForecastAsync(
            Guid productId)
        {
            var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
            var endDate = DateHelper.GetPeriodEndDate(Period.Weekly);

            return await _fixture.Context.PurchaseForecasts
                .SingleAsync(x =>
                    x.ProductId == productId &&
                    x.WeekStartDate == startDate &&
                    x.WeekEndDate == endDate);
        }

        internal async Task<List<PurchaseForecast>> GetPurchaseForecastsAsync(Guid productId)
        {
            return await _fixture.Context.PurchaseForecasts
                .Where(x => x.ProductId == productId)
                .ToListAsync();
        }
    }
}
