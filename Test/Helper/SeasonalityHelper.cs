using Domain.Entities.Core;
using Domain.Entities.Derived;
using Domain.Enums;

namespace Test.Helper
{
    internal static class SeasonalityHelper
    {
        internal static Seasonality CreateSeasonality(
            Product product,
            decimal factor
        )
        {
            return new Seasonality
            {
                ProductId = product.Id,
                Product = product,
                PeriodStartDate = DateHelper.GetPeriodStartDate(Period.Monthly),
                Factor = factor
            };
        }

        //internal static List<Seasonality> CreateSeasonality(
        //    Product product,
        //    decimal[]? factors
        //)
        //{
        //    List<Seasonality> seasonalities = new List<Seasonality>();

        //    factors = factors ?? [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];

        //    foreach (var factor in factors)
        //    {
        //        var monthStart = DateHelper.GetPeriodStartDate(Period.Monthly);

        //        var seasonality = new Seasonality
        //        {
        //            ProductId = product.Id,
        //            Product = product,
        //            PeriodStartDate = monthStart,
        //            Factor = factor
        //        };

        //        seasonalities.Add(seasonality);
        //    }

        //    return seasonalities;
        //}
    }
}
