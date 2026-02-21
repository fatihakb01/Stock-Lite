namespace Infrastructure.Services.Scheduler.Imports.Records;

public class UserDefinedFieldRecord
{
    public required string SKU { get; set; }
    public required string Key { get; set; }
    public string? Value { get; set; }
}
