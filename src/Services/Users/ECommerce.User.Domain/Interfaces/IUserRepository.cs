namespace ECommerce.User.Domain.Interfaces;

public interface IUserRepository
{
    Task<Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Entities.User> CreateAsync(Entities.User user, CancellationToken cancellationToken = default);
    Task<Entities.User> UpdateAsync(Entities.User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
