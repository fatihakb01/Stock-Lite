using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class ForecastHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal static Forecast CreateForecast(
            Product product,
            Period period,
            decimal predictedQuantity = 100,
            decimal predictedRevenue = 1000,
            DateTime? periodStartDate = null,
            DateTime? periodEndDate = null
        ) 
        {
            var forecast = new Forecast
            {
                Product = product,
                ProductId = product.Id,
                PeriodType = period,
                PeriodStartDate = periodStartDate ?? DateHelper.GetPeriodStartDate(period),
                PeriodEndDate = periodEndDate ?? DateHelper.GetPeriodEndDate(period),
                PredictedQuantity = predictedQuantity,
                PredictedRevenue = predictedRevenue,
                Confidence = 0.50
            };

            return forecast;
        }

        internal async Task<Forecast> GetForecastAsync(
            Guid productId,
            Period period)
        {
            var correctDate = DateHelper.GetPeriodStartDate(period);

            return await _fixture.Context.Forecasts
                .SingleAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period &&
                    x.PeriodStartDate == correctDate);
        }

        internal async Task<int> GetForecastCountAsync(
            Guid productId,
            Period? period)
        {

            if (period == null)
            {
                return await _fixture.Context.Forecasts
                    .CountAsync(x => x.ProductId == productId);
            }

            return await _fixture.Context.Forecasts
                .CountAsync(x =>
                    x.ProductId == productId &&
                    x.PeriodType == period
                );
        }

        internal async Task SeedForecastDependenciesAsync(
            Product product,
            int quantity,
            decimal revenue,
            Period period)
        {
            var aggregate = SalesAggregateHelper.CreateSalesAggregate(product, period, quantity, revenue);
            var seasonality = SeasonalityHelper.CreateSeasonality(product, 1);

            await _fixture.Context.SalesAggregates.AddAsync(aggregate);
            await _fixture.Context.Seasonality.AddAsync(seasonality);
        }
    }
}
