using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Interfaces;

namespace ECommerce.Product.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
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
        if (product == null || product.IsDeleted)
            return null;
            
        return _mapper.Map<ProductDto?>(product);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug);
        if (product == null || product.IsDeleted)
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
        // Validate slug uniqueness
        if (await _productRepository.ExistsBySlugAsync(dto.Slug))
            throw new InvalidOperationException("Product with this slug already exists");

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        var product = _mapper.Map<Domain.Entities.Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.IsDeleted = false;
        product.IsPublished = dto.IsActive;
        product.PublishedAt = dto.IsActive ? DateTime.UtcNow : null;

        // Update stock status based on inventory
        UpdateStockStatus(product);

        await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(string id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException("Product not found");

        // Validate slug uniqueness
        if (dto.Slug != product.Slug && await _productRepository.ExistsBySlugAsync(dto.Slug, id))
            throw new InvalidOperationException("Product with this slug already exists");

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;

        // Update published status
        if (dto.IsActive && !product.IsPublished)
        {
            product.IsPublished = true;
            product.PublishedAt = DateTime.UtcNow;
        }
        else if (!dto.IsActive)
        {
            product.IsPublished = false;
        }

        // Update stock status
        UpdateStockStatus(product);

        await _productRepository.UpdateAsync(id, product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException("Product not found");

        // Soft delete
        product.DeletedAt = DateTime.UtcNow;
        product.IsDeleted = true;
        product.IsActive = false;
        product.IsPublished = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(id, product);
    }

    public async Task<PagedResultDto<ProductListDto>> SearchAndFilterAsync(ProductSearchDto searchDto)
    {
        // Validate and sanitize input
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
            searchDto.IsActive,
            searchDto.SortBy,
            searchDto.SortOrder,
            searchDto.PageNumber,
            searchDto.PageSize
        );

        var productDtos = new List<ProductListDto>();
        foreach (var product in items)
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            var dto = _mapper.Map<ProductListDto>(product);
            
            // Create new record with category name
            productDtos.Add(dto with { CategoryName = category?.Name ?? "Unknown" });
        }

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
        var productDtos = new List<ProductListDto>();

        foreach (var product in products)
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            var dto = _mapper.Map<ProductListDto>(product);
            productDtos.Add(dto with { CategoryName = category?.Name ?? "Unknown" });
        }

        return productDtos;
    }

    public async Task<IEnumerable<ProductListDto>> GetRelatedProductsAsync(string productId, int limit = 5)
    {
        var products = await _productRepository.GetRelatedProductsAsync(productId, limit);
        var productDtos = new List<ProductListDto>();

        foreach (var product in products)
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            var dto = _mapper.Map<ProductListDto>(product);
            productDtos.Add(dto with { CategoryName = category?.Name ?? "Unknown" });
        }

        return productDtos;
    }

    public async Task<bool> UpdateStockAsync(string id, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            throw new KeyNotFoundException("Product not found");

        if (quantity < 0)
            throw new InvalidOperationException("Stock quantity cannot be negative");

        var result = await _productRepository.UpdateStockAsync(id, quantity);
        
        if (result)
        {
            // Update stock status
            product.Inventory.Stock = quantity;
            UpdateStockStatus(product);
            await _productRepository.UpdateAsync(id, product);
        }

        return result;
    }

    private static void UpdateStockStatus(Domain.Entities.Product product)
    {
        if (!product.Inventory.TrackInventory)
        {
            product.Status = Domain.Enums.ProductStatus.Active;
            return;
        }

        if (product.Inventory.Stock <= 0)
        {
            product.Status = Domain.Enums.ProductStatus.OutOfStock;
        }
        else if (product.Inventory.Stock <= product.Inventory.LowStockThreshold)
        {
            product.Status = Domain.Enums.ProductStatus.LowStock;
        }
        else
        {
            product.Status = Domain.Enums.ProductStatus.Active;
        }
    }
}
