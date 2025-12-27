using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Entities.Derived;

public class ConfirmedOrderAggregate
{
    public Guid ProductId { get; set; }

    public Period PeriodType { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }

    public int TotalConfirmedQuantity { get; set; }
    public decimal TotalConfirmedRevenue { get; set; }

    // Navigation
    public required Product Product { get; set; }
}
