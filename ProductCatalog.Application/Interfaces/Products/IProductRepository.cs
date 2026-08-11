using ProductCatalog.Application.DTOs.Common;
using ProductCatalog.Application.DTOs.Products;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Interfaces.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);

    Task<IReadOnlyList<Product>> GetAllAsync();

    Task AddAsync(Product product);

    void Update(Product product);

    void Delete(Product product);

    Task SaveChangesAsync();

    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
    ProductQueryDto query);
}
