namespace Infrastructure.Services.Export.Records
{
    public sealed class PurchaseForecastRecord
    {
        public required string SKU { get; set; }

        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int WeekIndex { get; set; }

        public decimal ForecastQuantity { get; set; }
        public int QuantityCommitted { get; set; }
        public int QuantityOnOrder { get; set; }
        public decimal ProjectedStock { get; set; }

        public bool NeedsReorder { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public DateTime? RecommendedOrderDate { get; set; }
    }
}
