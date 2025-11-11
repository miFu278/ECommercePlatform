using AutoMapper;
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Domain.Interfaces;

namespace ECommerce.User.Application.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SessionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, string? currentRefreshToken = null, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var sessions = user.Sessions
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.LastAccessedAt)
            .Select(s => new UserSessionDto(
                s.Id,
                s.DeviceInfo,
                s.IpAddress,
                s.UserAgent,
                s.IsActive,
                s.ExpiresAt,
                s.CreatedAt,
                s.LastAccessedAt,
                s.IsExpired,
                !string.IsNullOrEmpty(currentRefreshToken) && s.RefreshToken == currentRefreshToken
            ))
            .ToList();

        return sessions;
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var session = user.Sessions.FirstOrDefault(s => s.Id == sessionId);
        
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        // Verify session belongs to user
        if (session.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to revoke this session");

        session.IsActive = false;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllSessionsAsync(Guid userId, string? exceptRefreshToken = null, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");

        foreach (var session in user.Sessions)
        {
            // Skip current session if exceptRefreshToken is provided
            if (!string.IsNullOrEmpty(exceptRefreshToken) && session.RefreshToken == exceptRefreshToken)
                continue;

            session.IsActive = false;
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
