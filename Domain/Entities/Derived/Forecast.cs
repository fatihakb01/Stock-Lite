using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Entities.Derived;

public class Forecast
{
    public Guid ProductId { get; set; }
    public Period PeriodType { get; set; } 
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public decimal PredictedQuantity { get; set; }
    public decimal PredictedRevenue { get; set; }
    public double Confidence { get; set; } 
    public required Product Product { get; set; }
}
