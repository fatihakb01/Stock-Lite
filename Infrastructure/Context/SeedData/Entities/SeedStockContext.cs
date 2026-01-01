namespace Infrastructure.Context.SeedData.Entities;

public sealed class SeedStockContext
{
    public Dictionary<Guid, int> OnHand { get; } = new Dictionary<Guid, int>();
    public Dictionary<Guid, int> OnOrder { get; } = new Dictionary<Guid, int>();
    public Dictionary<Guid, int> Committed { get; } = new Dictionary<Guid, int>();
}
