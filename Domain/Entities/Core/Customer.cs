namespace Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = "";   
    public string? LegalName { get; set; }          
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public Guid CompanyId { get; set; }

    // Navigation
    public required Company Company { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
