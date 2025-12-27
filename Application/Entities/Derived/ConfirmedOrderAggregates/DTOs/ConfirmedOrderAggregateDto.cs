using Domain.Enums;

namespace Application.Entities.Derived.ConfirmedOrderAggregates.DTOs;

public class ConfirmedOrderAggregateDto
{
    public Period PeriodType { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int TotalConfirmedQuantity { get; set; }
    public decimal TotalConfirmedRevenue { get; set; }
}
