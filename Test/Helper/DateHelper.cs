using Domain.Enums;

namespace Test.Helper;

internal static class DateHelper
{
    internal static DateTime GetUtcToday()
    {
        return DateTime.UtcNow.Date;
    }

    internal static DateTime GetStartOfWeekUtc()
    {
        var today = DateTime.UtcNow.Date;

        int diff = ((int)today.DayOfWeek + 6) % 7;

        return today.AddDays(-diff);
    }

    internal static DateTime GetStartOfMonthUtc()
    {
        var today = DateTime.UtcNow.Date;

        return new DateTime(today.Year, today.Month, 1);
    }

    internal static DateTime? GetPeriodStartDate(Period? period)
    {
        return period switch
        {
            Period.Daily => GetUtcToday(),
            Period.Weekly => GetStartOfWeekUtc(),
            Period.Monthly => GetStartOfMonthUtc(),
            _ => GetUtcToday()
        };
    }
}