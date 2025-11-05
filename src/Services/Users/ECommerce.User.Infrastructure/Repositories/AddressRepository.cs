using ECommerce.User.Domain.Entities;
using ECommerce.User.Domain.Interfaces;
using ECommerce.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.User.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly UserDbContext _context;

    public AddressRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetDefaultAddressAsync(Guid userId, string addressType, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.AddressType == addressType && a.IsDefault, cancellationToken);
    }

    public async Task<Address> CreateAsync(Address address, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);
        return address;
    }

    public async Task<Address> UpdateAsync(Address address, CancellationToken cancellationToken = default)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync(cancellationToken);
        return address;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await GetByIdAsync(id, cancellationToken);
        if (address != null)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> BelongsToUserAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .AnyAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);
    }

    public async Task UnsetDefaultAddressesAsync(Guid userId, string addressType, CancellationToken cancellationToken = default)
    {
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId && a.AddressType == addressType && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var address in addresses)
        {
            address.IsDefault = false;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
