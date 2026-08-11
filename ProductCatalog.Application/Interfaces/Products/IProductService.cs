using ProductCatalog.Application.DTOs.Common;
using ProductCatalog.Application.DTOs.Products;

namespace ProductCatalog.Application.Interfaces.Products;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);

    Task<IReadOnlyList<ProductDto>> GetAllAsync();

    Task<ProductDto> CreateAsync(CreateProductDto request);

    Task<bool> UpdateAsync(int id, UpdateProductDto request);

    Task<bool> DeleteAsync(int id);

    Task<PagedResultDto<ProductDto>> GetPagedAsync(ProductQueryDto query);
}
