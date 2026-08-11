namespace ProductCatalog.Application.DTOs.Products;

public class UpdateProductDto
{
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public int Stock { get; set; }
}