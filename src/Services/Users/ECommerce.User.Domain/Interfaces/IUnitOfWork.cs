using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.User.Domain.Interfaces;

public interface IUnitOfWork : ECommerce.Shared.Abstractions.Repositories.IUnitOfWork
{
    IUserRepository Users { get; }
    IAddressRepository Addresses { get; }
}
