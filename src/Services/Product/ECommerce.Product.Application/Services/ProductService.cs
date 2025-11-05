using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Interfaces;

namespace ECommerce.Product.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
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

        var product = _mapper.Map<Domain.Entities.Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.IsDeleted = false;

        await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(string id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        if (dto.Slug != product.Slug && await _productRepository.ExistsBySlugAsync(dto.Slug, id))
            throw new InvalidOperationException("Product with this slug already exists");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(id, product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        // Soft delete
        product.DeletedAt = DateTime.UtcNow;
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(id, product);
    }
}
