using Domain.Entities.Derived;

namespace Domain.Entities.Core;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string SKU { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Foreign Key
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public ProductInventoryPolicy? InventoryPolicy { get; set; }
    public StockLevel? StockLevel { get; set; }
    public ICollection<PurchaseForecast> PurchaseForecasts { get; set; } = new List<PurchaseForecast>();
    public ICollection<SalesAggregate> SalesAggregates { get; set; } = new List<SalesAggregate>();
    public ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();
    public ICollection<Seasonality> Seasonality { get; set; } = new List<Seasonality>();
    public ICollection<ConfirmedOrderAggregate> ConfirmedOrderAggregates { get; set; } = new List<ConfirmedOrderAggregate>();
    public ICollection<TransactionLineItem> TransactionLineItems { get; set; } = new List<TransactionLineItem>();
    public ICollection<PurchaseOrderLineItem> PurchaseOrderLineItem { get; set; } = new List<PurchaseOrderLineItem>();
    public ICollection<ConfirmedOrderLineItem> ConfirmedOrderLineItem { get; set; } = new List<ConfirmedOrderLineItem>();
    public ICollection<UserDefinedField> UserDefinedFields { get; set; } = new List<UserDefinedField>();
}
