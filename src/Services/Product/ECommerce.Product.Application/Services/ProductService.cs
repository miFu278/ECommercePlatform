using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Enums;
using ECommerce.Product.Domain.Interfaces;

namespace ECommerce.Product.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IImageService imageService,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _imageService = imageService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.DeletedAt != null)
            return null;

        return _mapper.Map<ProductDto?>(product);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug);
        if (product == null || product.DeletedAt != null)
            return null;

        return _mapper.Map<ProductDto?>(product);
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string query)
    {
        var products = await _productRepository.SearchAsync(query);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(string categoryId)
    {
        var products = await _productRepository.GetByCategoryIdAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }


    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _productRepository.ExistsBySlugAsync(dto.Slug))
            throw new InvalidOperationException("Product with this slug already exists");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        var product = _mapper.Map<Domain.Entities.Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.Status = ProductStatus.Draft;
        product.CategoryPath = category.Path ?? new List<string>();

        await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(string id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.DeletedAt != null)
            throw new KeyNotFoundException("Product not found");

        if (dto.Slug != product.Slug && await _productRepository.ExistsBySlugAsync(dto.Slug, id))
            throw new InvalidOperationException("Product with this slug already exists");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;
        product.CategoryPath = category.Path ?? new List<string>();

        if (dto.Status == ProductStatus.Active && !product.IsPublished)
        {
            product.IsPublished = true;
            product.PublishedAt = DateTime.UtcNow;
        }
        else if (dto.Status != ProductStatus.Active)
        {
            product.IsPublished = false;
        }

        await _productRepository.UpdateAsync(id, product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.DeletedAt != null)
            throw new KeyNotFoundException("Product not found");

        product.DeletedAt = DateTime.UtcNow;
        product.Status = ProductStatus.Archived;
        product.IsPublished = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(id, product);
    }


    public async Task<PagedResultDto<ProductListDto>> SearchAndFilterAsync(ProductSearchDto searchDto)
    {
        searchDto.PageNumber = Math.Max(1, searchDto.PageNumber);
        searchDto.PageSize = Math.Clamp(searchDto.PageSize, 1, 100);

        var (items, totalCount) = await _productRepository.SearchAndFilterAsync(
            searchDto.SearchTerm,
            searchDto.CategoryId,
            searchDto.MinPrice,
            searchDto.MaxPrice,
            searchDto.Tags,
            searchDto.InStock,
            searchDto.IsFeatured,
            searchDto.Status,
            searchDto.SortBy,
            searchDto.SortOrder,
            searchDto.PageNumber,
            searchDto.PageSize
        );

        // Batch load categories to avoid N+1 query
        var categoryIds = items.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
        var categoryDict = categories.ToDictionary(c => c.Id!, c => c.Name);

        var productDtos = items.Select(product =>
        {
            var dto = _mapper.Map<ProductListDto>(product);
            var categoryName = categoryDict.GetValueOrDefault(product.CategoryId, "Unknown");
            return dto with { CategoryName = categoryName };
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

        return new PagedResultDto<ProductListDto>
        {
            Items = productDtos,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<ProductListDto>> GetFeaturedAsync(int limit = 10)
    {
        var products = await _productRepository.GetFeaturedAsync(limit);

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
        var categoryDict = categories.ToDictionary(c => c.Id!, c => c.Name);

        return products.Select(product =>
        {
            var dto = _mapper.Map<ProductListDto>(product);
            var categoryName = categoryDict.GetValueOrDefault(product.CategoryId, "Unknown");
            return dto with { CategoryName = categoryName };
        }).ToList();
    }

    public async Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(string productId, int limit = 5)
    {
        var products = await _productRepository.GetRelatedProductsAsync(productId, limit);

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
        var categoryDict = categories.ToDictionary(c => c.Id!, c => c.Name);

        return products.Select(product =>
        {
            var dto = _mapper.Map<ProductListDto>(product);
            var categoryName = categoryDict.GetValueOrDefault(product.CategoryId, "Unknown");
            return dto with { CategoryName = categoryName };
        }).ToList();
    }

    public async Task<bool> UpdateStockAsync(string id, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.DeletedAt != null)
            throw new KeyNotFoundException("Product not found");

        if (quantity < 0)
            throw new InvalidOperationException("Stock quantity cannot be negative");

        return await _productRepository.UpdateStockAsync(id, quantity);
    }

    public async Task<string> AddProductImageAsync(string productId, Stream fileStream, string fileName, string contentType)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || product.DeletedAt != null)
            throw new KeyNotFoundException("Product not found");

        // Upload to Cloudinary
        var imageUrl = await _imageService.UploadImageAsync(fileStream, fileName, contentType);

        // Add image to product
        if (product.Images == null)
            product.Images = new List<Domain.ValueObjects.ProductImage>();

        var productImage = new Domain.ValueObjects.ProductImage
        {
            Url = imageUrl,
            AltText = product.Name,
            IsPrimary = product.Images.Count == 0,
            Order = product.Images.Count
        };

        product.Images.Add(productImage);
        product.UpdatedAt = DateTime.UtcNow;

        // Update product
        await _productRepository.UpdateAsync(productId, product);

        return imageUrl;
    }

    public async Task DeleteProductImageAsync(string productId, string imageUrl)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || product.DeletedAt != null)
            throw new KeyNotFoundException("Product not found");

        // Check if image exists in product
        var imageToRemove = product.Images?.FirstOrDefault(img => img.Url == imageUrl);
        if (imageToRemove == null)
            throw new InvalidOperationException("Image not found in product");

        // Extract publicId from Cloudinary URL and delete
        var publicId = ExtractPublicIdFromUrl(imageUrl);
        if (!string.IsNullOrEmpty(publicId))
        {
            await _imageService.DeleteImageAsync(publicId);
        }

        // Remove image from product
        product.Images!.Remove(imageToRemove);
        
        // Reorder remaining images
        for (int i = 0; i < product.Images.Count; i++)
        {
            product.Images[i].Order = i;
        }
        
        // If removed image was primary, set first image as primary
        if (imageToRemove.IsPrimary && product.Images.Count > 0)
        {
            product.Images[0].IsPrimary = true;
        }

        product.UpdatedAt = DateTime.UtcNow;

        // Update product
        await _productRepository.UpdateAsync(productId, product);
    }

    private string? ExtractPublicIdFromUrl(string imageUrl)
    {
        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/{cloud_name}/image/upload/{transformations}/{public_id}.{format}
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');

            // Find "upload" segment and get everything after it
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
            {
                var pathAfterUpload = string.Join("/", segments.Skip(uploadIndex + 1));
                // Remove file extension
                var lastDotIndex = pathAfterUpload.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    return pathAfterUpload.Substring(0, lastDotIndex);
                }
                return pathAfterUpload;
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        return null;
    }
}
