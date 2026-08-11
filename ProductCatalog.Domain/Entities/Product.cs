namespace ProductCatalog.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
