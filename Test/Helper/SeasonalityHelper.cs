using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;

namespace Test.Helper
{
    internal static class SeasonalityHelper
    {
        internal static Seasonality CreateSeasonality(
            Product product,
            decimal factor,
            DateTime? periodStartDate = null
        )
        {
            return new Seasonality
            {
                ProductId = product.Id,
                Product = product,
                PeriodStartDate = periodStartDate ?? DateHelper.GetPeriodStartDate(Period.Monthly),
                Factor = factor
            };
        }
    }
}
