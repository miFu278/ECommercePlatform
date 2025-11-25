using ECommerce.Payment.Domain.Interfaces;
using ECommerce.Payment.Infrastructure.Repositories;

namespace ECommerce.Payment.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;
    private IPaymentRepository? _payments;

    public UnitOfWork(PaymentDbContext context)
    {
        _context = context;
    }

    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
