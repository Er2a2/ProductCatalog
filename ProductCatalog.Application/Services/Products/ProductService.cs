using ProductCatalog.Application.DTOs.Common;
using ProductCatalog.Application.DTOs.Products;
using ProductCatalog.Application.Interfaces.Caching;
using ProductCatalog.Application.Interfaces.Products;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Services.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ICacheLock _cacheLock;

    public ProductService(
        IProductRepository productRepository,
        ICacheService cacheService,
        ICacheLock cacheLock)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _cacheLock = cacheLock;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var key = $"product:{id}";

        //First Check
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(key);

        if (cachedProduct is not null)
        {
            Console.WriteLine("✅ Product loaded from Redis");
            return cachedProduct;
        }

        var cacheLock = _cacheLock.GetLock(key);

        await cacheLock.WaitAsync();

        try
        {
            // Double-check cache after acquiring lock
            cachedProduct = await _cacheService.GetAsync<ProductDto>(key);

            if (cachedProduct is not null)
            {
                Console.WriteLine("✅ Product loaded from Redis after waiting");
                return cachedProduct;
            }

            Console.WriteLine($"🗄️ DB request for product {id}");

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                return null;
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Stock = product.Stock
            };

            await _cacheService.SetAsync(
                key,
                productDto,
                TimeSpan.FromMinutes(5));

            return productDto;
        }

        finally
        {
            cacheLock.Release();
        }
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products
            .Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Stock = product.Stock
            })
            .ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto request)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return false;
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.Description = request.Description;
        product.Stock = request.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync($"product:{id}");

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return false;
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync($"product:{id}");


        return true;
    }

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(
    int page,
    int pageSize)
    {
        var (products, totalCount) =
            await _productRepository.GetPagedAsync(page, pageSize);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var items = products.Select(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock
        });

        return new PagedResultDto<ProductDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
