using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common.Responses;
using ProductCatalog.Application.DTOs.Common;
using ProductCatalog.Application.DTOs.Products;
using ProductCatalog.Application.Interfaces.Products;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] PaginationRequestDto request)
    {
        var result = await _productService.GetPagedAsync(
            request.Page,
            request.PageSize);

        return Ok(
            ApiResponse<PagedResultDto<ProductDto>>.Ok(
                result,
                "Products retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound(
                 ApiResponse<object>.Fail("Product not found."));
        }

        return Ok(
       ApiResponse<ProductDto>.Ok(
           product,
           "Product retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto request)
    {
        var product = await _productService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ApiResponse<ProductDto>.Ok(
                product,
                "Product created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
    int id,
    UpdateProductDto request)
    {
        var updated = await _productService.UpdateAsync(id, request);

        if (!updated)
        {
            return NotFound(
                ApiResponse<object>.Fail("Product not found."));
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(
                ApiResponse<object>.Fail("Product not found."));
        }

        return NoContent();
    }
}