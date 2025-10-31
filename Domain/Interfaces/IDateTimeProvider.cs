namespace Domain.Interfaces;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }

    int GetIsoWeekOfYear(DateTime date);
    int GetIsoYear(DateTime date);
    (DateTime Start, DateTime End) GetWeekRange(DateTime date);
    (DateTime Start, DateTime End) GetMonthRange(DateTime date);
}
