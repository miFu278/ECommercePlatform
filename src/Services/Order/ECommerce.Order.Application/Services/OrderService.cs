using AutoMapper;
using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Domain.Entities;
using ECommerce.Order.Domain.Enums;
using ECommerce.Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IShoppingCartService _cartService;
    private readonly IPaymentService _paymentService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IShoppingCartService cartService,
        IPaymentService paymentService,
        IEventBus eventBus,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cartService = cartService;
        _paymentService = paymentService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        // Get cart items
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        // Validate stock availability
        var unavailableItems = cart.Items.Where(i => !i.IsAvailable || i.StockQuantity < i.Quantity).ToList();
        if (unavailableItems.Any())
        {
            throw new InvalidOperationException(
                $"Some items are not available: {string.Join(", ", unavailableItems.Select(i => i.ProductName))}");
        }

        // Create order
        var order = _mapper.Map<Domain.Entities.Order>(dto);
        order.Id = Guid.NewGuid();
        order.UserId = userId;
        order.OrderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();
        order.Status = OrderStatus.Pending;
        order.PaymentStatus = PaymentStatus.Pending;
        order.CreatedAt = DateTime.UtcNow;

        // Add items from cart
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductName,
                Sku = cartItem.Sku,
                ImageUrl = cartItem.ImageUrl,
                UnitPrice = cartItem.Price,
                Quantity = cartItem.Quantity,
                Discount = 0,
                TotalPrice = cartItem.Price * cartItem.Quantity,
                CreatedAt = DateTime.UtcNow
            };
            order.Items.Add(orderItem);
        }

        // Calculate totals
        order.Subtotal = order.Items.Sum(i => i.TotalPrice);
        order.ShippingCost = CalculateShippingCost(order.Subtotal);
        order.Tax = CalculateTax(order.Subtotal);
        order.Discount = 0;
        order.TotalAmount = order.Subtotal + order.ShippingCost + order.Tax - order.Discount;

        // Add initial status history
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = OrderStatus.Pending,
            Notes = "Order created",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Save order
        await _unitOfWork.Orders.CreateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Clear cart
        await _cartService.ClearCartAsync(userId);

        // Publish event
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Items = order.Items.Select(i => new ECommerce.EventBus.Events.OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.UnitPrice
            }).ToList()
        };
        await _eventBus.PublishAsync(orderCreatedEvent);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await _unitOfWork.Orders.GetCountByUserIdAsync(userId);

        return new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(page, pageSize);
        var totalCount = await _unitOfWork.Orders.GetTotalCountAsync();

        return new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, int page = 1, int pageSize = 10)
    {
        var orders = await _unitOfWork.Orders.GetByStatusAsync(status, page, pageSize);
        var totalCount = await _unitOfWork.Orders.GetTotalCountAsync();

        return new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        // Validate status transition
        ValidateStatusTransition(order.Status, dto.Status);

        // Update status
        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // Update tracking number if provided
        if (!string.IsNullOrEmpty(dto.TrackingNumber))
        {
            order.TrackingNumber = dto.TrackingNumber;
        }

        // Update timestamps based on status
        switch (dto.Status)
        {
            case OrderStatus.Shipped:
                order.ShippedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = dto.Notes;
                break;
        }

        // Add status history
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = dto.Status,
            Notes = dto.Notes,
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task CancelOrderAsync(Guid id, string reason)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        // Only pending or processing orders can be cancelled
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot cancel order with status {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;
        order.UpdatedAt = DateTime.UtcNow;

        // Add status history
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            Notes = reason,
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
    }

    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
            { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
            { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Refunded } },
            { OrderStatus.Cancelled, new List<OrderStatus>() },
            { OrderStatus.Refunded, new List<OrderStatus>() }
        };

        if (!validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {currentStatus} to {newStatus}");
        }
    }

    private decimal CalculateShippingCost(decimal subtotal)
    {
        // Free shipping for orders over $100
        if (subtotal >= 100)
            return 0;

        // Standard shipping
        return 10.00m;
    }

    private decimal CalculateTax(decimal subtotal)
    {
        // 10% tax
        return subtotal * 0.10m;
    }

    public async Task<OrderStatisticsDto> GetStatisticsAsync()
    {
        var allOrders = await _unitOfWork.Orders.GetAllAsync(1, int.MaxValue);
        var today = DateTime.UtcNow.Date;
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var statistics = new OrderStatisticsDto
        {
            TotalOrders = allOrders.Count(),
            PendingOrders = allOrders.Count(o => o.Status == OrderStatus.Pending),
            ProcessingOrders = allOrders.Count(o => o.Status == OrderStatus.Processing),
            ShippedOrders = allOrders.Count(o => o.Status == OrderStatus.Shipped),
            DeliveredOrders = allOrders.Count(o => o.Status == OrderStatus.Delivered),
            CancelledOrders = allOrders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = allOrders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            TodayRevenue = allOrders.Where(o => o.CreatedAt >= today && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            ThisMonthRevenue = allOrders.Where(o => o.CreatedAt >= thisMonth && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            TodayOrders = allOrders.Count(o => o.CreatedAt >= today),
            ThisMonthOrders = allOrders.Count(o => o.CreatedAt >= thisMonth)
        };

        return statistics;
    }

    public async Task<OrderDashboardDto> GetDashboardAsync()
    {
        var statistics = await GetStatisticsAsync();

        // Get recent orders
        var recentOrders = await _unitOfWork.Orders.GetAllAsync(1, 10);
        var recentOrderDtos = recentOrders.Select(o => new RecentOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.ShippingFullName,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        }).ToList();

        // Get top products
        var allOrders = await _unitOfWork.Orders.GetAllAsync(1, int.MaxValue);
        var topProducts = allOrders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(10)
            .ToList();

        return new OrderDashboardDto
        {
            Statistics = statistics,
            RecentOrders = recentOrderDtos,
            TopProducts = topProducts
        };
    }
}