using Domain.Entities.Core;
using Domain.Enums;

namespace Domain.Entities.Derived;

public class ProductKpi
{
    public JobExecutionStatus SchedulerStatus { get; set; }
    public int TotalProductCount { get; set; }
    public int BelowSafetyStockCount { get; set; }
    public int RecommendedOrdersCount { get; set; }
    public int WithoutStockAtLeadTimeCount { get; set; }
    public int WithoutForecastCount { get; set; }

    public Guid CompanyId { get; set; }
    public required Company Company { get; set; }
}
