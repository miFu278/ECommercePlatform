using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Domain.Enums;

namespace ECommerce.Order.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<OrderDto?> GetOrderByIdAsync(Guid id);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
    Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10);
    Task<PagedResultDto<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, int page = 1, int pageSize = 10);
    Task<OrderDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto);
    Task CancelOrderAsync(Guid id, string reason);
    Task<OrderStatisticsDto> GetStatisticsAsync();
    Task<OrderDashboardDto> GetDashboardAsync();
}
