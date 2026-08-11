namespace ProductCatalog.Application.DTOs.Products;

public class ProductQueryDto
{
    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}