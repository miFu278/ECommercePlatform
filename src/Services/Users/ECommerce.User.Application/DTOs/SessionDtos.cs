namespace ECommerce.User.Application.DTOs;

public record UserSessionDto(
    Guid Id,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent,
    bool IsActive,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime LastAccessedAt,
    bool IsExpired,
    bool IsCurrent
);
