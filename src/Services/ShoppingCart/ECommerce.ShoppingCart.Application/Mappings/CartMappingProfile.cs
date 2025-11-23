using AutoMapper;
using ECommerce.ShoppingCart.Application.DTOs;
using ECommerce.ShoppingCart.Domain.Entities;

namespace ECommerce.ShoppingCart.Application.Mappings;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<Domain.Entities.ShoppingCart, CartDto>();
        CreateMap<CartItem, CartItemDto>();
    }
}
