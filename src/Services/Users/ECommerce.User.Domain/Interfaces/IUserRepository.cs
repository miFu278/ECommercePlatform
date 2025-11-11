using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.User.Domain.Interfaces;

public interface IUserRepository : IRepository<Entities.User>
{
    Task<Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
