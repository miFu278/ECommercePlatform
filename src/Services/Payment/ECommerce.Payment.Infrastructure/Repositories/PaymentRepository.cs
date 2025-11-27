using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;
using ECommerce.Payment.Domain.Interfaces;
using ECommerce.Payment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .Include(p => p.History.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PaymentEntity?> GetByPaymentNumberAsync(string paymentNumber)
    {
        return await _context.Payments
            .Include(p => p.History.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .Include(p => p.History.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<IEnumerable<PaymentEntity>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentEntity>> GetByStatusAsync(PaymentStatus status, int page = 1, int pageSize = 10)
    {
        return await _context.Payments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Payments.CountAsync();
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        return await _context.Payments.CountAsync(p => p.UserId == userId);
    }

    public async Task<PaymentEntity> CreateAsync(PaymentEntity payment)
    {
        await _context.Payments.AddAsync(payment);
        return payment;
    }

    public Task UpdateAsync(PaymentEntity payment)
    {
        _context.Payments.Update(payment);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Payments.AnyAsync(p => p.Id == id);
    }

    public async Task<string> GeneratePaymentNumberAsync()
    {
        var date = DateTime.UtcNow;
        var prefix = $"PAY{date:yyyyMMdd}";
        
        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastPayment != null)
        {
            var lastSequence = lastPayment.PaymentNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
