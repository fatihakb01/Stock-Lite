using Domain.Entities.Derived;

namespace Domain.Entities.Core;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<ConfirmedOrder> ConfirmedOrders { get; set; } = new List<ConfirmedOrder>();
    public ICollection<JobTask> JobTasks { get; set; } = new List<JobTask>();
    public ProductKpi? ProductKpi { get; set; }
}
