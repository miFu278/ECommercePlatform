using ECommerce.User.Application.DTOs;

namespace ECommerce.User.Application.Interfaces;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressDto?> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto dto, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken = default);
    Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
