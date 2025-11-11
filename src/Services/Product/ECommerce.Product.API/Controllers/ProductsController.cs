using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with advanced search and filtering
    /// </summary>
    /// <remarks>
    /// Example: GET /api/products?searchTerm=laptop&amp;categoryId=123&amp;minPrice=500&amp;maxPrice=2000&amp;sortBy=price&amp;sortOrder=asc&amp;pageNumber=1&amp;pageSize=20
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetAll([FromQuery] ProductSearchDto searchDto)
    {
        var result = await _productService.SearchAndFilterAsync(searchDto);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    /// <summary>
    /// Get product by slug (SEO-friendly URL)
    /// </summary>
    /// <remarks>
    /// Example: GET /api/products/slug/wireless-headphones-pro
    /// </remarks>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    /// <summary>
    /// Simple search products (legacy endpoint)
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Search query is required" });

        var products = await _productService.SearchAsync(query);
        return Ok(products);
    }

    /// <summary>
    /// Get featured products
    /// </summary>
    /// <param name="limit">Number of products to return (default: 10)</param>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetFeatured([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 50)
            return BadRequest(new { message = "Limit must be between 1 and 50" });

        var products = await _productService.GetFeaturedAsync(limit);
        return Ok(products);
    }

    /// <summary>
    /// Get related products (same category)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="limit">Number of products to return (default: 5)</param>
    [HttpGet("{id}/related")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetRelated(string id, [FromQuery] int limit = 5)
    {
        if (limit < 1 || limit > 20)
            return BadRequest(new { message = "Limit must be between 1 and 20" });

        try
        {
            var products = await _productService.GetRelatedProductsAsync(id, limit);
            return Ok(products);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string categoryId)
    {
        var products = await _productService.GetByCategoryIdAsync(categoryId);
        return Ok(products);
    }

    /// <summary>
    /// Create new product (Admin only)
    /// </summary>
    [HttpPost]
    // [Authorize(Roles = "Admin")] // Uncomment when authentication is integrated
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto request)
    {
        try
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update product (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    // [Authorize(Roles = "Admin")] // Uncomment when authentication is integrated
    public async Task<ActionResult<ProductDto>> Update(string id, [FromBody] UpdateProductDto request)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, request);
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update product stock (Admin only)
    /// </summary>
    [HttpPatch("{id}/stock")]
    // [Authorize(Roles = "Admin")] // Uncomment when authentication is integrated
    public async Task<ActionResult> UpdateStock(string id, [FromBody] UpdateStockDto request)
    {
        try
        {
            var result = await _productService.UpdateStockAsync(id, request.Quantity);
            
            if (!result)
                return BadRequest(new { message = "Failed to update stock" });

            return Ok(new { message = "Stock updated successfully", quantity = request.Quantity });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete product - soft delete (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")] // Uncomment when authentication is integrated
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// DTO for updating stock
/// </summary>
public record UpdateStockDto(int Quantity);
