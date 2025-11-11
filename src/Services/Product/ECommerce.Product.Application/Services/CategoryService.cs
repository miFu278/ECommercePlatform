using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Interfaces;

namespace ECommerce.Product.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return _mapper.Map<CategoryDto?>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(string parentId)
    {
        var categories = await _categoryRepository.GetChildCategoriesAsync(parentId);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.ExistsBySlugAsync(dto.Slug))
            throw new InvalidOperationException("Category with this slug already exists");

        var category = new Domain.Entities.Category
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            ParentId = dto.ParentId,
            ImageUrl = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsVisible = dto.IsVisible,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _categoryRepository.CreateAsync(category);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(string id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        if (dto.Slug != category.Slug && await _categoryRepository.ExistsBySlugAsync(dto.Slug, id))
            throw new InvalidOperationException("Category with this slug already exists");

        category.Name = dto.Name;
        category.Slug = dto.Slug;
        category.Description = dto.Description;
        category.ParentId = dto.ParentId;
        category.ImageUrl = dto.ImageUrl;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsVisible = dto.IsVisible;
        category.MetaTitle = dto.MetaTitle;
        category.MetaDescription = dto.MetaDescription;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(id, category);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        // Business rule: Cannot delete category with children
        var children = await _categoryRepository.GetChildCategoriesAsync(id);
        if (children.Any())
            throw new InvalidOperationException("Cannot delete category with children");

        await _categoryRepository.DeleteAsync(id);
    }
}
