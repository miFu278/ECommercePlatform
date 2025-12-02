using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Domain.Enums;
using ECommerce.Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Application.EventHandlers;

public class PaymentCompletedEventHandler : IEventHandler<PaymentCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<PaymentCompletedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentCompletedEvent @event)
    {
        _logger.LogInformation("Handling PaymentCompletedEvent for Order: {OrderId}", @event.OrderId);

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(@event.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderId}", @event.OrderId);
                return;
            }

            // Update payment status
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentTransactionId = @event.PaymentId.ToString();
            order.UpdatedAt = DateTime.UtcNow;

            // Update order status to Processing
            if (order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Processing;

                // Add status history
                order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Status = OrderStatus.Processing,
                    Notes = $"Payment completed. Payment ID: {@event.PaymentId}",
                    ChangedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Order {OrderNumber} updated to Processing after payment completion", order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle PaymentCompletedEvent for Order: {OrderId}", @event.OrderId);
        }
    }
}
