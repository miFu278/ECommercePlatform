using ECommerce.Product.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IImageService imageService, ILogger<ImagesController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ImageUploadResponse>> UploadImage(IFormFile file, [FromQuery] string folder = "products")
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            using var stream = file.OpenReadStream();
            var url = await _imageService.UploadImageAsync(stream, file.FileName, file.ContentType, folder);
            return Ok(new ImageUploadResponse(url, "Upload successful"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");
            return StatusCode(500, new { message = "Upload failed" });
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteImage([FromQuery] string publicId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest(new { message = "PublicId is required" });

            var result = await _imageService.DeleteImageAsync(publicId);
            if (!result)
                return NotFound(new { message = "Image not found or already deleted" });

            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image");
            return StatusCode(500, new { message = "Delete failed" });
        }
    }
}

public record ImageUploadResponse(string Url, string Message);
public record MultipleImageUploadResponse(List<string> Urls, string Message);
