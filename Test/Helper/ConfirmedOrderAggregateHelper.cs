using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class ConfirmedOrderAggregateHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;

        internal async Task<ConfirmedOrderAggregate> CreateConfirmedOrderAggregate(
            Product product,
            Period period,
            int totalConfirmedQuantity = 10,
            decimal totalConfirmedRevenue = 1000,
            DateTime? periodStartDate = null,
            DateTime? periodEndDate = null,
            bool saveChanges = true
        )
        {
            var co = new ConfirmedOrderAggregate
            {
                Product = product,
                ProductId = product.Id,
                PeriodType = period,
                PeriodStartDate = periodStartDate ?? DateHelper.GetPeriodStartDate(period),
                PeriodEndDate = periodEndDate ?? DateHelper.GetPeriodEndDate(period),
                TotalConfirmedQuantity = totalConfirmedQuantity,
                TotalConfirmedRevenue = totalConfirmedRevenue
            };

            await _fixture.Context.ConfirmedOrderAggregates.AddAsync(co);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return co;
        }

        internal async Task<ConfirmedOrderAggregate> GetConfirmedOrderAggregateAsync(
            Guid productId,
            Period period)
        {
            var correctDate = DateHelper.GetPeriodStartDate(period);

            return await _fixture.Context.ConfirmedOrderAggregates
                .SingleAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period &&
                    x.PeriodStartDate == correctDate);
        }

        internal async Task<int> GetConfirmedOrderAggregatesCountAsync(
            Guid productId,
            Period? period)
        {
            if (period == null)
            {
                return await _fixture.Context.ConfirmedOrderAggregates
                    .CountAsync(x => x.ProductId == productId);
            }

            return await _fixture.Context.ConfirmedOrderAggregates
                .CountAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period
                );
        }
    }
}
