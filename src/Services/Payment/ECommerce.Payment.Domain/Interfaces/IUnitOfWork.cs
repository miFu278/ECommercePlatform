namespace ECommerce.Payment.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
