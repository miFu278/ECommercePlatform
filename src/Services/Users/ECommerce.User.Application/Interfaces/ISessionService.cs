using ECommerce.User.Application.DTOs;

namespace ECommerce.User.Application.Interfaces;

public interface ISessionService
{
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, string? currentRefreshToken = null, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task RevokeAllSessionsAsync(Guid userId, string? exceptRefreshToken = null, CancellationToken cancellationToken = default);
}
