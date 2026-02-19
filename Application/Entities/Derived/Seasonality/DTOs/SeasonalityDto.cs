namespace Application.Entities.Derived.Seasonality.DTOs;

public class SeasonalityDto
{
    public Guid ProductId { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public decimal Factor { get; set; }
}
