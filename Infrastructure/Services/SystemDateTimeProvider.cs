using System.Globalization;
using Domain.Interfaces;

namespace Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public int GetIsoWeekOfYear(DateTime date) => ISOWeek.GetWeekOfYear(date);
    public int GetIsoYear(DateTime date) => ISOWeek.GetYear(date);

    public (DateTime Start, DateTime End) GetWeekRange(DateTime date)
    {
        var year = ISOWeek.GetYear(date);
        var week = ISOWeek.GetWeekOfYear(date);
        var start = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        var end = start.AddDays(6);
        return (start, end);
    }

    public (DateTime Start, DateTime End) GetMonthRange(DateTime date)
    {
        var start = new DateTime(date.Year, date.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }
}
