namespace Domain.Enums;

public enum ProductFilter
{
    FailedJobs = 0,
    BelowSafetyStock = 1,
    WithRecommendedOrders = 2,
    WithoutStockAtLeadTime = 3,
    WithoutForecast = 4
}
