namespace ECommerce.Product.Domain.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string folder = "products");
    Task<bool> DeleteImageAsync(string publicId);
}
