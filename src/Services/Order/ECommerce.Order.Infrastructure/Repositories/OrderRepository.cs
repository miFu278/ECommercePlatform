using ECommerce.Order.Domain.Entities;
using ECommerce.Order.Domain.Enums;
using ECommerce.Order.Domain.Interfaces;
using ECommerce.Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Order?> GetByIdAsync(Guid id, bool includeItems = true)
    {
        var query = _context.Orders.AsQueryable();

        if (includeItems)
        {
            query = query
                .Include(o => o.Items)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        return await query.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Domain.Entities.Order?> GetByOrderNumberAsync(string orderNumber, bool includeItems = true)
    {
        var query = _context.Orders.AsQueryable();

        if (includeItems)
        {
            query = query
                .Include(o => o.Items)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }

        return await query.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetByStatusAsync(OrderStatus status, int page = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        return await _context.Orders.CountAsync(o => o.UserId == userId);
    }

    public async Task<Domain.Entities.Order> CreateAsync(Domain.Entities.Order order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }

    public Task UpdateAsync(Domain.Entities.Order order)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id, false);
        if (order != null)
        {
            _context.Orders.Remove(order);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"ORD{date:yyyyMMdd}";
        
        // Get the last order number for today
        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastOrder != null)
        {
            // Extract sequence number from last order
            var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
