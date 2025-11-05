using ECommerce.User.Domain.Entities;

namespace ECommerce.User.Domain.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Address?> GetDefaultAddressAsync(Guid userId, string addressType, CancellationToken cancellationToken = default);
    Task<Address> CreateAsync(Address address, CancellationToken cancellationToken = default);
    Task<Address> UpdateAsync(Address address, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> BelongsToUserAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);
    Task UnsetDefaultAddressesAsync(Guid userId, string addressType, CancellationToken cancellationToken = default);
}
