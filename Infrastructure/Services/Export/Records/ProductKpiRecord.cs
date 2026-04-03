namespace Infrastructure.Services.Export.Records
{
    public sealed class ProductKpiRecord
    {
        public required string SchedulerStatus { get; set; }
        public int TotalProductCount { get; set; }
        public int BelowSafetyStockCount { get; set; }
        public int RecommendedOrdersCount { get; set; }
        public int WithoutStockAtLeadTimeCount { get; set; }
        public int WithoutForecastCount { get; set; }
    }
}
