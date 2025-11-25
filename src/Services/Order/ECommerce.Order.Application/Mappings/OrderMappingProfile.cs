using AutoMapper;
using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Domain.Entities;

namespace ECommerce.Order.Application.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Order -> OrderDto
        CreateMap<Domain.Entities.Order, OrderDto>()
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PaymentStatusDisplay, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
            .ForMember(dest => dest.PaymentMethodDisplay, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity)))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => new ShippingAddressDto
            {
                FullName = src.ShippingFullName,
                Phone = src.ShippingPhone,
                AddressLine1 = src.ShippingAddressLine1,
                AddressLine2 = src.ShippingAddressLine2,
                City = src.ShippingCity,
                State = src.ShippingState,
                PostalCode = src.ShippingPostalCode,
                Country = src.ShippingCountry
            }));

        // OrderItem -> OrderItemDto
        CreateMap<OrderItem, OrderItemDto>();

        // CreateOrderDto -> Order
        CreateMap<CreateOrderDto, Domain.Entities.Order>()
            .ForMember(dest => dest.ShippingFullName, opt => opt.MapFrom(src => src.ShippingAddress.FullName))
            .ForMember(dest => dest.ShippingPhone, opt => opt.MapFrom(src => src.ShippingAddress.Phone))
            .ForMember(dest => dest.ShippingAddressLine1, opt => opt.MapFrom(src => src.ShippingAddress.AddressLine1))
            .ForMember(dest => dest.ShippingAddressLine2, opt => opt.MapFrom(src => src.ShippingAddress.AddressLine2))
            .ForMember(dest => dest.ShippingCity, opt => opt.MapFrom(src => src.ShippingAddress.City))
            .ForMember(dest => dest.ShippingState, opt => opt.MapFrom(src => src.ShippingAddress.State))
            .ForMember(dest => dest.ShippingPostalCode, opt => opt.MapFrom(src => src.ShippingAddress.PostalCode))
            .ForMember(dest => dest.ShippingCountry, opt => opt.MapFrom(src => src.ShippingAddress.Country));
    }
}
