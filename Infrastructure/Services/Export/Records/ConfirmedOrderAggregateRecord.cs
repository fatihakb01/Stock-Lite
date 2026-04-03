namespace Infrastructure.Services.Export.Records
{
    public sealed class ConfirmedOrderAggregateRecord
    {
        public required string SKU { get; set; }
        public required string PeriodType { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public int TotalConfirmedQuantity { get; set; }
        public decimal TotalConfirmedRevenue { get; set; }
    }
}
