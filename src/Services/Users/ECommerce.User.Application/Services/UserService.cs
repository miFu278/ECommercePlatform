using AutoMapper;
using ECommerce.Common.Exceptions;
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Domain.Interfaces;

namespace ECommerce.User.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException("User not found");

        // Check if username is being changed and if it's already taken
        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
        {
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(dto.Username, cancellationToken);
            if (existingUser != null)
                throw new BusinessException("Username is already taken", "USERNAME_EXISTS");
            
            user.Username = dto.Username;
        }

        // Check if phone number is being changed and if it's already taken
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
        {
            var existingUser = await _unitOfWork.Users.GetByPhoneNumberAsync(dto.PhoneNumber, cancellationToken);
            if (existingUser != null)
                throw new ConflictException("Phone number is already registered", "PHONE_EXISTS");
        }

        // Chỉ update những field có giá trị (không null)
        if (dto.FirstName != null)
            user.FirstName = dto.FirstName;
            
        if (dto.LastName != null)
            user.LastName = dto.LastName;
            
        if (dto.PhoneNumber != null)
            user.PhoneNumber = dto.PhoneNumber;
            
        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth;

        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload to get updated navigation properties
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException("User not found");

        // Verify current password
        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            throw new BusinessException("Current password is incorrect", "INVALID_PASSWORD");

        // Validate new password is different
        if (_passwordHasher.VerifyPassword(dto.NewPassword, user.PasswordHash))
            throw new BusinessException("New password must be different from current password", "SAME_PASSWORD");

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Invalidate all sessions for security
        foreach (var session in user.Sessions)
        {
            session.IsActive = false;
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAccountAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException("User not found");

        // Verify password before deletion
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            throw new BusinessException("Password is incorrect", "INVALID_PASSWORD");

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
            throw new NotFoundException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}
