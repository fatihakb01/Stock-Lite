using Domain.Enums;

namespace Application.Entities.Derived.Forecasts.DTOs;

public class ForecastDto 
{
    public Guid ProductId { get; set; }
    public Period PeriodType { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public double PredictedQuantity { get; set; }
    public double PredictedRevenue { get; set; }
    public double Confidence { get; set; }
}
