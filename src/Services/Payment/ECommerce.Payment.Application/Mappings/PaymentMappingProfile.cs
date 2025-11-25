using AutoMapper;
using ECommerce.Payment.Application.DTOs;

namespace ECommerce.Payment.Application.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Domain.Entities.Payment, PaymentDto>()
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MethodDisplay, opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(dest => dest.ProviderDisplay, opt => opt.MapFrom(src => src.Provider.ToString()));
    }
}
