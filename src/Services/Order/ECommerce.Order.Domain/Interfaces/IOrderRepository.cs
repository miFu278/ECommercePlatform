using ECommerce.Order.Domain.Enums;
using OrderEntity = ECommerce.Order.Domain.Entities.Order;

namespace ECommerce.Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdAsync(Guid id, bool includeItems = true);
    Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber, bool includeItems = true);
    Task<IEnumerable<OrderEntity>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<IEnumerable<OrderEntity>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<IEnumerable<OrderEntity>> GetByStatusAsync(OrderStatus status, int page = 1, int pageSize = 10);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByUserIdAsync(Guid userId);
    Task<OrderEntity> CreateAsync(OrderEntity order);
    Task UpdateAsync(OrderEntity order);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<string> GenerateOrderNumberAsync();
}
