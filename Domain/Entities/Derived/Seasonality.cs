using Domain.Entities.Core;

namespace Domain.Entities.Derived;

public class Seasonality
{
    public Guid ProductId { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public decimal Factor { get; set; }

    public required Product Product { get; set; }
}
