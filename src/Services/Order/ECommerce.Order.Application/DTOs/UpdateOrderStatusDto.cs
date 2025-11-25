using ECommerce.Order.Domain.Enums;

namespace ECommerce.Order.Application.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
}
