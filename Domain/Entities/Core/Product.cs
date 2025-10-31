namespace Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string SKU { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Foreign Key
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public StockLevel? StockLevel { get; set; }
    public ICollection<SalesAggregate> SalesAggregates { get; set; } = new List<SalesAggregate>();
    public ICollection<TransactionLineItem> TransactionLineItems { get; set; } = new List<TransactionLineItem>();
}
