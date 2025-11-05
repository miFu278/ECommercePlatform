using AutoMapper;
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Domain.Entities;
using ECommerce.User.Domain.Interfaces;

namespace ECommerce.User.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AddressService(
        IAddressRepository addressRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _addressRepository = addressRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var addresses = await _addressRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto?> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        
        if (address == null)
            return null;

        // Verify address belongs to user
        if (address.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to access this address");

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> CreateAddressAsync(Guid userId, CreateAddressDto dto, CancellationToken cancellationToken = default)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // If this is set as default, unset other default addresses of the same type
        if (dto.IsDefault)
        {
            await _addressRepository.UnsetDefaultAddressesAsync(userId, dto.AddressType, cancellationToken);
        }

        var address = _mapper.Map<Address>(dto);
        address.UserId = userId;
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.CreateAsync(address, cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        
        if (address == null)
            throw new KeyNotFoundException("Address not found");

        // Verify address belongs to user
        if (address.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to update this address");

        // If this is set as default, unset other default addresses of the same type
        if (dto.IsDefault && !address.IsDefault)
        {
            await _addressRepository.UnsetDefaultAddressesAsync(userId, dto.AddressType, cancellationToken);
        }

        // Update address
        _mapper.Map(dto, address);
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.UpdateAsync(address, cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }

    public async Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        
        if (address == null)
            throw new KeyNotFoundException("Address not found");

        // Verify address belongs to user
        if (address.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to delete this address");

        await _addressRepository.DeleteAsync(addressId, cancellationToken);
    }

    public async Task<AddressDto> SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);
        
        if (address == null)
            throw new KeyNotFoundException("Address not found");

        // Verify address belongs to user
        if (address.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to update this address");

        // Unset other default addresses of the same type
        await _addressRepository.UnsetDefaultAddressesAsync(userId, address.AddressType, cancellationToken);

        // Set this as default
        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.UpdateAsync(address, cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}
