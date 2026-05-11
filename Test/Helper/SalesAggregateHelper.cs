using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class SalesAggregateHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal static SalesAggregate CreateSalesAggregate(
            Product product,
            Period period,
            int totalQuantity = 100,
            decimal totalRevenue = 1000
        ) => new SalesAggregate
        {
            ProductId = product.Id,
            Product = product,
            PeriodType = period,
            PeriodStartDate = DateHelper.GetPeriodStartDate(period),
            PeriodEndDate = DateHelper.GetPeriodEndDate(period),
            TotalQuantity = totalQuantity,
            TotalRevenue = totalRevenue
        };

        internal async Task<SalesAggregate> GetSalesAsync(
            Guid productId,
            Period period)
        {
            var correctDate = DateHelper.GetPeriodStartDate(period);

            return await _fixture.Context.SalesAggregates
                .SingleAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period &&
                    x.PeriodStartDate == correctDate);
        }

        internal async Task<int> GetSalesCountAsync(
            Guid productId,
            Period? period)
        {

            if (period == null)
            {
                return await _fixture.Context.SalesAggregates
                    .CountAsync(x => x.ProductId == productId);
            }

            return await _fixture.Context.SalesAggregates
                .CountAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period
                );
        }
    }
}
