using ECommerce.User.Application.DTOs;

namespace ECommerce.User.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAccountAsync(Guid userId, string password, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
