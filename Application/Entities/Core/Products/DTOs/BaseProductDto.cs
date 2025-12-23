namespace Application.Entities.Core.Products.DTOs;

public class BaseProductDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
}
