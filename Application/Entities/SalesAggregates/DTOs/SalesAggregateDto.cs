using Domain.Enums;

namespace Application.Entities.SalesAggregates.DTOs;

public class SalesAggregateDto
{
    public Period PeriodType { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
