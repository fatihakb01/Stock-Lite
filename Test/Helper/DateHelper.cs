using Domain.Enums;

namespace Test.Helper;

internal static class DateHelper
{
    private static DateTime GetUtcToday()
    {
        return DateTime.UtcNow.Date;
    }

    private static DateTime GetStartOfWeekUtc(bool start)
    {
        var today = DateTime.UtcNow.Date;

        int value = start ?
            -(((int)today.DayOfWeek + 6) % 7) :
            (7 - (int)today.DayOfWeek);

        return today.AddDays(value);
    }

    private static DateTime GetStartOfMonthUtc(bool start)
    {
        var today = DateTime.UtcNow.Date;

        int value = start ? 1 : DateTime.DaysInMonth(today.Year, today.Month);

        return new DateTime(today.Year, today.Month, value);
    }

    internal static DateTime GetPeriodStartDate(Period? period)
    {
        return period switch
        {
            Period.Daily => GetUtcToday(),
            Period.Weekly => GetStartOfWeekUtc(true),
            Period.Monthly => GetStartOfMonthUtc(true),
            _ => GetUtcToday()
        };
    }

    internal static DateTime GetPeriodEndDate(Period? period)
    {
        return period switch
        {
            Period.Daily => GetUtcToday(),
            Period.Weekly => GetStartOfWeekUtc(false),
            Period.Monthly => GetStartOfMonthUtc(false),
            _ => GetUtcToday()
        };
    }
}