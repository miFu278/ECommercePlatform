using ECommerce.Order.Domain.Enums;
using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.Order.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    
    // Navigation
    public Order Order { get; set; } = null!;
}
