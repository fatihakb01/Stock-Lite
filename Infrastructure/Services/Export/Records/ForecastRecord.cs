namespace Infrastructure.Services.Export.Records
{
    public sealed class ForecastRecord
    {
        public required string SKU { get; set; }
        public required string PeriodType { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal PredictedQuantity { get; set; }
        public decimal PredictedRevenue { get; set; }
    }
}
