using ECommerce.User.Domain.Interfaces;
using ECommerce.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.User.Infrastructure.Repositories;

public class UserRepository : Repository<Domain.Entities.User>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.User?> CheckUniquenessAsync(string email, string username, string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u =>
                u.Email == email ||
                (!string.IsNullOrEmpty(username) && u.Username == username) ||
                (!string.IsNullOrEmpty(phoneNumber) && u.PhoneNumber == phoneNumber),
                cancellationToken);
    }

    public override async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Addresses)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .CountAsync(cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Sessions)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Sessions.Any(s => s.RefreshToken == refreshToken), cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, cancellationToken);
    }
}
