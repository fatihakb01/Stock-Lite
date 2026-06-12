using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;
using Test.Integration.Helper;

namespace Test.Helper
{
    internal class SeasonalityHelper(DatabaseFixture fixture)
    {
        private readonly DatabaseFixture _fixture = fixture;
        internal async Task<Seasonality> CreateSeasonality(
            Product product,
            decimal factor,
            DateTime? periodStartDate = null,
            bool saveChanges = true
        )
        {
            var seasonality = new Seasonality
            {
                ProductId = product.Id,
                Product = product,
                PeriodStartDate = periodStartDate ?? DateHelper.GetPeriodStartDate(Period.Monthly),
                Factor = factor
            };

            await _fixture.Context.Seasonality.AddAsync(seasonality);

            if (saveChanges)
                await _fixture.Context.SaveChangesAsync();

            return seasonality;
        }
    }
}
