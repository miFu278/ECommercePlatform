using AutoMapper;
using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Domain.ValueObjects;

namespace ECommerce.Product.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Product mappings
        CreateMap<Domain.Entities.Product, ProductDto>();
        
        CreateMap<Domain.Entities.Product, ProductListDto>()
            .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src => 
                src.Images.FirstOrDefault(i => i.IsPrimary) != null 
                    ? src.Images.First(i => i.IsPrimary).Url 
                    : src.Images.FirstOrDefault() != null ? src.Images.First().Url : null))
            .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => 
                !src.Inventory.TrackInventory || src.Inventory.Stock > 0))
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Inventory.Stock))
            .ForMember(dest => dest.CategoryName, opt => opt.Ignore()); // Set manually in service
        
        CreateMap<CreateProductDto, Domain.Entities.Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryPath, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.ProductStatus.Draft))
            .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<UpdateProductDto, Domain.Entities.Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Sku, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryPath, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
            .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // Value Object mappings
        CreateMap<ProductImage, ProductImageDto>().ReverseMap();
        CreateMap<ProductAttribute, ProductAttributeDto>().ReverseMap();
        CreateMap<ProductSpecifications, ProductSpecificationsDto>().ReverseMap();
        CreateMap<ProductSeo, ProductSeoDto>().ReverseMap();
        CreateMap<ProductInventory, ProductInventoryDto>().ReverseMap();
        CreateMap<ProductDimensions, ProductDimensionsDto>().ReverseMap();
        CreateMap<ProductRating, ProductRatingDto>().ReverseMap();

        // Category mappings
        CreateMap<Domain.Entities.Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Domain.Entities.Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        CreateMap<UpdateCategoryDto, Domain.Entities.Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Tag mappings
        CreateMap<Domain.Entities.Tag, TagDto>();
        CreateMap<CreateTagDto, Domain.Entities.Tag>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
