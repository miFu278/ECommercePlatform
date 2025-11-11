using AutoMapper;
using ECommerce.User.Application.DTOs;

namespace ECommerce.User.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> UserDto
        CreateMap<Domain.Entities.User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => 
                src.UserRoles
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role.Name)
                    .ToList()));

        // RegisterDto -> User
        CreateMap<RegisterDto, Domain.Entities.User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // UpdateProfileDto -> User
        CreateMap<UpdateProfileDto, Domain.Entities.User>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Address mappings
        CreateMap<Domain.Entities.Address, AddressDto>()
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.FullAddress));

        CreateMap<CreateAddressDto, Domain.Entities.Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateAddressDto, Domain.Entities.Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
