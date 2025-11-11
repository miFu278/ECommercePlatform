using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetById(string id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        
        if (tag == null)
            return NotFound(new { message = "Tag not found" });

        return Ok(tag);
    }

    /// <summary>
    /// Get tag by name
    /// </summary>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<TagDto>> GetByName(string name)
    {
        var tag = await _tagService.GetByNameAsync(name);
        
        if (tag == null)
            return NotFound(new { message = "Tag not found" });

        return Ok(tag);
    }

    /// <summary>
    /// Create new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto request)
    {
        try
        {
            var tag = await _tagService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update tag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> Update(string id, [FromBody] UpdateTagDto request)
    {
        try
        {
            var tag = await _tagService.UpdateAsync(id, request);
            return Ok(tag);
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
    /// Delete tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
