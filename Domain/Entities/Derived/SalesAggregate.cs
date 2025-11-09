using Domain.Enums;

namespace Domain.Entities;

public class SalesAggregate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Period PeriodType { get; set; } = Period.Daily;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; } 
    

    // Navigation
    public required Product Product { get; set; }
}
