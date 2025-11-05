using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get root categories (no parent)
    /// </summary>
    [HttpGet("root")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetRootCategories()
    {
        var categories = await _categoryService.GetRootCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        
        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }

    /// <summary>
    /// Get child categories
    /// </summary>
    [HttpGet("{parentId}/children")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetChildren(string parentId)
    {
        var children = await _categoryService.GetChildCategoriesAsync(parentId);
        return Ok(children);
    }

    /// <summary>
    /// Create new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(string id, [FromBody] UpdateCategoryDto request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request);
            return Ok(category);
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
    /// Delete category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
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
}
