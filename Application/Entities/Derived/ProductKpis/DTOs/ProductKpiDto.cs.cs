namespace Application.Entities.Derived.ProductKpis.DTOs;

public class ProductKpiDto
{
    public bool SchedulerFailed { get; set; }
    public int TotalProductCount { get; set; }
    public int BelowSafetyStockCount { get; set; }
    public int RecommendedOrdersCount { get; set; }
    public int WithoutStockAtLeadTimeCount { get; set; }
    public int WithoutForecastCount { get; set; }
}
