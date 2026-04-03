namespace Infrastructure.Services.Export.Records
{
    public sealed class SalesAggregateRecord
    {
        public required string SKU { get; set; }
        public required string PeriodType { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
