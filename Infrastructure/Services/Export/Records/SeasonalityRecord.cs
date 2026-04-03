namespace Infrastructure.Services.Export.Records
{
    public sealed class SeasonalityRecord
    {
        public required string SKU { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public decimal PredictedQuantity { get; set; }
        public decimal Factor { get; set; }
    }
}
