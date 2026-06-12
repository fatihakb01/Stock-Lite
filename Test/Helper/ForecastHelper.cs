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

        internal async Task<Forecast> CreateForecast(
            Product product,
            Period period,
            decimal predictedQuantity = 100,
            decimal predictedRevenue = 1000,
            double confidence = 0.50,
            DateTime? periodStartDate = null,
            DateTime? periodEndDate = null,
            bool saveChanges = true
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
                Confidence = confidence
            };

            await _fixture.Context.Forecasts.AddAsync(forecast);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

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

        internal async Task<List<Forecast>> SeedWeeklyForecastsAsync(
            Product product, 
            decimal predictedQuantity = 100,
            decimal predictedRevenue = 1000
        )
        {
            var startDate = DateHelper.GetPeriodStartDate(Period.Weekly);
            var endDate = DateHelper.GetPeriodEndDate(Period.Weekly);

            var forecasts = Enumerable.Range(0, 52).Select(i => new Forecast
            {
                ProductId = product.Id,
                Product = product,
                PeriodType = Period.Weekly,
                PeriodStartDate = startDate.AddDays(i * 7),
                PeriodEndDate = endDate.AddDays(i * 7),
                PredictedQuantity = predictedQuantity,
                PredictedRevenue = predictedRevenue,
                Confidence = 0.50
            }).ToList();

            await _fixture.Context.Forecasts.AddRangeAsync(forecasts);
            await _fixture.Context.SaveChangesAsync();

            return forecasts;
        }
    }
}
