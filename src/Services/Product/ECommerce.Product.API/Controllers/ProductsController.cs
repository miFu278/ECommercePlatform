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

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetAll([FromQuery] ProductSearchDto searchDto)
    {
        var result = await _productService.SearchAndFilterAsync(searchDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(string id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDto>> GetBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Search query is required" });

        var products = await _productService.SearchAsync(query);
        return Ok(products);
    }


    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetFeatured([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 50)
            return BadRequest(new { message = "Limit must be between 1 and 50" });

        var products = await _productService.GetFeaturedAsync(limit);
        return Ok(products);
    }

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

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string categoryId)
    {
        var products = await _productService.GetByCategoryIdAsync(categoryId);
        return Ok(products);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpPatch("{id}/stock")]
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

    [HttpDelete("{id}")]
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

    [HttpPost("{id}/images")]
    public async Task<ActionResult> UploadProductImage(string id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            using var stream = file.OpenReadStream();
            var imageUrl = await _productService.AddProductImageAsync(id, stream, file.FileName, file.ContentType);

            return Ok(new { 
                message = "Image uploaded successfully", 
                imageUrl
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for product {ProductId}", id);
            return StatusCode(500, new { message = "Failed to upload image", error = ex.Message });
        }
    }

    [HttpDelete("{id}/images")]
    public async Task<ActionResult> DeleteProductImage(string id, [FromQuery] string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return BadRequest(new { message = "Image URL is required" });

            await _productService.DeleteProductImageAsync(id, imageUrl);

            return Ok(new { message = "Image deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image for product {ProductId}", id);
            return StatusCode(500, new { message = "Failed to delete image", error = ex.Message });
        }
    }

    /// <summary>
    /// Seed sample products (Development only)
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult> SeedProducts([FromServices] ECommerce.Product.Infrastructure.Data.IMongoDbContext context)
    {
        try
        {
            var seeder = new ECommerce.Product.Infrastructure.Data.ProductSeeder(context);
            await seeder.SeedAsync();
            return Ok(new { message = "Products seeded successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record UpdateStockDto(int Quantity);
