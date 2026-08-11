using ProductCatalog.Application.DTOs.Common;
using ProductCatalog.Application.DTOs.Products;
using ProductCatalog.Application.Interfaces.Caching;
using ProductCatalog.Application.Interfaces.Products;
using ProductCatalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ProductCatalog.Application.Services.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ICacheLock _cacheLock;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICacheService cacheService,
        ICacheLock cacheLock,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _cacheLock = cacheLock;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var key = $"product:{id}";

        //First Check
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(key);

        if (cachedProduct is not null)
        {
            _logger.LogInformation("Product {ProductId} loaded from Redis", id);
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
                _logger.LogInformation(
                  "Product {ProductId} loaded from Redis after waiting",
                  id);
                return cachedProduct;
            }

            _logger.LogInformation(
              "Product {ProductId} loaded from database",
              id);

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

        await _cacheService.RemoveByPatternAsync("products:*");

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
        await _cacheService.RemoveByPatternAsync("products:*");

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
        await _cacheService.RemoveByPatternAsync("products:*");


        return true;
    }

    private static string BuildProductListCacheKey(ProductQueryDto query)
    {
        return $"products:" +
               $"page={query.Page}:" +
               $"size={query.PageSize}:" +
               $"search={query.Search?.Trim().ToLowerInvariant()}:" +
               $"sort={query.SortBy?.Trim().ToLowerInvariant()}:" +
               $"order={query.SortOrder?.Trim().ToLowerInvariant()}";
    }

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(
    ProductQueryDto query)
    {
        var cacheKey = BuildProductListCacheKey(query);

        var cachedResult =
            await _cacheService.GetAsync<PagedResultDto<ProductDto>>(cacheKey);

        if (cachedResult is not null)
        {
            _logger.LogInformation("Product list loaded from Redis");
            return cachedResult;
        }

        _logger.LogInformation("Product list loaded from database");

        var (products, totalCount) =
            await _productRepository.GetPagedAsync(query);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)query.PageSize);

        var items = products.Select(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock
        });

        var result = new PagedResultDto<ProductDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }
}
