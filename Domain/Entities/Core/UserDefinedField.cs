namespace Domain.Entities.Core;

public class UserDefinedField
{
    public Guid ProductId { get; set; }
    public Guid CompanyId { get; set; }
    public required Product Product { get; set; }

    public required string Key { get; set; }
    public string? Value { get; set; }
}
