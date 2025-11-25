using ECommerce.Order.Domain.Enums;

namespace ECommerce.Order.Application.DTOs;

public class CreateOrderDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string? CustomerNotes { get; set; }
}
