using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Interfaces;

namespace ECommerce.Product.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;

    public TagService(ITagRepository tagRepository, IMapper mapper)
    {
        _tagRepository = tagRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TagDto>>(tags);
    }

    public async Task<TagDto?> GetByIdAsync(string id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        return _mapper.Map<TagDto?>(tag);
    }

    public async Task<TagDto?> GetByNameAsync(string name)
    {
        var tag = await _tagRepository.GetByNameAsync(name);
        return _mapper.Map<TagDto?>(tag);
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto)
    {
        var existing = await _tagRepository.GetBySlugAsync(dto.Slug);
        if (existing != null)
            throw new InvalidOperationException("Tag with this slug already exists");

        var tag = new Domain.Entities.Tag
        {
            Name = dto.Name,
            Slug = dto.Slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tagRepository.CreateAsync(tag);
        return _mapper.Map<TagDto>(tag);
    }

    public async Task<TagDto> UpdateAsync(string id, UpdateTagDto dto)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found");

        tag.Name = dto.Name;
        tag.Slug = dto.Slug;
        tag.UpdatedAt = DateTime.UtcNow;

        await _tagRepository.UpdateAsync(id, tag);
        return _mapper.Map<TagDto>(tag);
    }

    public async Task DeleteAsync(string id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found");

        await _tagRepository.DeleteAsync(id);
    }
}
