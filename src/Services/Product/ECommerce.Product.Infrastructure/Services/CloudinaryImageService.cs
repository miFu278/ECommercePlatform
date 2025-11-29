using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Configuration;

namespace ECommerce.Product.Infrastructure.Services;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(CloudinarySettings settings)
    {
        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string folder = "products")
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is empty");

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(contentType.ToLower()))
            throw new ArgumentException("Only JPEG, PNG, WebP images are allowed");

        // Validate file size (max 5MB)
        if (fileStream.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File size must not exceed 5MB");

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            Transformation = new Transformation()
                .Width(1200)
                .Height(1200)
                .Crop("limit")
                .Quality("auto")
                .FetchFormat("auto"),
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
