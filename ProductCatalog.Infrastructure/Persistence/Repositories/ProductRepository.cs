using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.DTOs.Products;
using ProductCatalog.Application.Interfaces.Products;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductCatalogDbContext _context;

    public ProductRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
    ProductQueryDto query)
    {
        var products = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            products = products.Where(p =>
                p.Name.Contains(query.Search) ||
                p.Description.Contains(query.Search));
        }

        products = query.SortBy?.ToLower() switch
        {
            "name" => query.SortOrder?.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Name)
                : products.OrderBy(p => p.Name),

            "price" => query.SortOrder?.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Price)
                : products.OrderBy(p => p.Price),

            "stock" => query.SortOrder?.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Stock)
                : products.OrderBy(p => p.Stock),

            _ => products.OrderBy(p => p.Id)
        };

        var totalCount = await products.CountAsync();

        var items = await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
