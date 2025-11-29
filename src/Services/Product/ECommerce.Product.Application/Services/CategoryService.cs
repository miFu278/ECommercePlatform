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

        var category = _mapper.Map<Domain.Entities.Category>(dto);
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        // Calculate level and path based on parent
        if (!string.IsNullOrEmpty(dto.ParentId))
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentId);
            if (parent != null)
            {
                category.Level = parent.Level + 1;
                category.Path = new List<string>(parent.Path) { parent.Id! };
            }
        }
        else
        {
            category.Level = 0;
            category.Path = new List<string>();
        }

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

        _mapper.Map(dto, category);
        category.UpdatedAt = DateTime.UtcNow;

        // Recalculate level and path if parent changed
        if (dto.ParentId != category.ParentId)
        {
            if (!string.IsNullOrEmpty(dto.ParentId))
            {
                var parent = await _categoryRepository.GetByIdAsync(dto.ParentId);
                if (parent != null)
                {
                    category.Level = parent.Level + 1;
                    category.Path = new List<string>(parent.Path) { parent.Id! };
                }
            }
            else
            {
                category.Level = 0;
                category.Path = new List<string>();
            }
        }

        await _categoryRepository.UpdateAsync(id, category);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        var children = await _categoryRepository.GetChildCategoriesAsync(id);
        if (children.Any())
            throw new InvalidOperationException("Cannot delete category with children");

        await _categoryRepository.DeleteAsync(id);
    }
}
