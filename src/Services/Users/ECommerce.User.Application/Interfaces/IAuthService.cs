using ECommerce.User.Application.DTOs;

namespace ECommerce.User.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
}
