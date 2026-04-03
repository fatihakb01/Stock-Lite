namespace Infrastructure.Services.Export.Records
{
    public sealed class StockLevelRecord
    {
        public required string SKU { get; set; }

        public int QuantityOnHand { get; set; }
        public int QuantityOnOrder { get; set; }
        public int QuantityCommitted { get; set; }

        public decimal NetStockPosition { get; set; }
        public decimal ForecastQuantity { get; set; }

        public int ReorderPoint { get; set; }

        public bool NeedsReorder { get; set; }
        public int RecommendedOrderQuantity { get; set; }
    }
}
