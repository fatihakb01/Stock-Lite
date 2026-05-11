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
            decimal totalRevenue = 1000,
            DateTime? periodStartDate = null,
            DateTime? periodEndDate = null
        ) => new SalesAggregate
        {
            ProductId = product.Id,
            Product = product,
            PeriodType = period,
            PeriodStartDate = periodStartDate ?? DateHelper.GetPeriodStartDate(period),
            PeriodEndDate = periodEndDate ?? DateHelper.GetPeriodEndDate(period),
            TotalQuantity = totalQuantity,
            TotalRevenue = totalRevenue
        };

        internal static List<SalesAggregate> CreateSalesAggregates(
            Product product,
            DateTime now,
            int months = 12
        )
        {
            List<SalesAggregate> aggregates = new List<SalesAggregate>();

            for (int i = 0; i < months; i++)
            {
                var start = new DateTime(now.Year, now.Month, 1).AddMonths(-i - 1);
                var quantity = i == 0 ? 1000 : 100;

                var aggregate = CreateSalesAggregate(
                    product,
                    Period.Monthly,
                    totalQuantity: quantity,
                    totalRevenue: quantity * 10m,
                    periodStartDate: start,
                    periodEndDate: start.AddMonths(1).AddDays(-1));

                aggregates.Add(aggregate);
            }

            return aggregates;
        }

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
