using AutoMapper;
using ECommerce.Common.Constants;
using ECommerce.Common.Exceptions;
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Domain.Entities;
using ECommerce.User.Domain.Interfaces;



namespace ECommerce.User.Application
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            _mapper = mapper;
        }
        public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
        {
            // Check if email exists
            if (await _userRepository.GetByEmailAsync(dto.Email, cancellationToken) != null)
            {
                throw new BusinessException("Email already registered", "EMAIL_EXISTS");
            }

            // Check if username exists (if provided)
            if (!string.IsNullOrEmpty(dto.Username))
            {
                if (await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken) != null)
                {
                    throw new BusinessException("Username already taken", "USERNAME_EXISTS");
                }
            }

            // Create user entity
            var user = _mapper.Map<Domain.Entities.User>(dto);
            user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
            user.EmailVerified = false;
            user.IsActive = true;

            // Generate email verification token
            user.EmailVerificationToken = _tokenService.GenerateEmailVerificationToken();
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            // Create user in database
            await _userRepository.CreateAsync(user, cancellationToken);

            user.UserRoles.Add(new Domain.Entities.UserRole
            {
                UserId = user.Id,
                RoleId = 3, // Customer role
                AssignedAt = DateTime.UtcNow
            });

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Reload user with roles to ensure navigation properties are loaded
            var createdUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken, cancellationToken);

            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            // Get user by email
            var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            // Check if account is locked
            if (user.IsLocked)
            {
                var remainingTime = user.LockoutEnd.HasValue
                    ? user.LockoutEnd.Value - DateTime.UtcNow
                    : TimeSpan.Zero;
                throw new BusinessException(
                    $"Account is locked. Try again in {remainingTime.Minutes} minutes.",
                    "ACCOUNT_LOCKED");
            }
            // Verify password
            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.IncrementFailedLoginAttempts(
                    AppConstants.Security.MaxFailedLoginAttempts,
                    AppConstants.Security.LockoutMinutes);
                await _userRepository.UpdateAsync(user, cancellationToken);

                throw new UnauthorizedException("Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
                throw new BusinessException("Account is inactive", "ACCOUNT_INACTIVE");

            // Reset failed attempts on successful login
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var session = new UserSession
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Security.RefreshTokenExpirationDays),
                IsActive = true
            };
            user.Sessions.Add(session);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Return LoginResponseDto
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = AppConstants.Security.AccessTokenExpirationMinutes * 60,
                User = _mapper.Map<UserDto>(user)
            };
        }
        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            // Find user by refresh token
            var users = await _userRepository.GetAllAsync(1, 1000, cancellationToken);
            var user = users.FirstOrDefault(u => u.Sessions.Any(s => s.RefreshToken == refreshToken && s.IsValid));

            if (user == null)
                throw new UnauthorizedException("Invalid refresh token");

            var session = user.Sessions.First(s => s.RefreshToken == refreshToken);

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update session
            session.RefreshToken = newRefreshToken;
            session.ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Security.RefreshTokenExpirationDays);
            session.LastAccessedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = AppConstants.Security.AccessTokenExpirationMinutes * 60,
                User = _mapper.Map<UserDto>(user)
            };
        }
        public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            // Find user by refresh token -> deactivate session
            var users = await _userRepository.GetAllAsync(1, 1000, cancellationToken);
            var user = users.FirstOrDefault(u => u.Sessions.Any(s => s.RefreshToken == refreshToken));

            if (user == null)
                return false;

            var session = user.Sessions.First(s => s.RefreshToken == refreshToken);
            session.IsActive = false;

            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
        {
            // Find user by verification token
            var users = await _userRepository.GetAllAsync(1, 10000, cancellationToken);
            var user = users.FirstOrDefault(u => u.EmailVerificationToken == token);

            if (user == null)
                throw new BusinessException("Invalid verification token", "INVALID_TOKEN");

            // Check if token is expired
            if (!user.IsEmailVerificationTokenValid)
                throw new BusinessException("Verification token has expired", "TOKEN_EXPIRED");

            // Verify email
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user == null)
            {
                // Don't reveal if email exists or not for security
                return true;
            }

            // Generate password reset token
            user.PasswordResetToken = _tokenService.GeneratePasswordResetToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Send password reset email
            await _emailService.SendPasswordResetAsync(user.Email, user.PasswordResetToken, cancellationToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user == null)
                throw new BusinessException("Invalid reset request", "INVALID_REQUEST");

            // Verify token
            if (user.PasswordResetToken != token)
                throw new BusinessException("Invalid reset token", "INVALID_TOKEN");

            // Check if token is expired
            if (!user.IsPasswordResetTokenValid)
                throw new BusinessException("Reset token has expired", "TOKEN_EXPIRED");

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            // Invalidate all existing sessions for security
            foreach (var session in user.Sessions)
            {
                session.IsActive = false;
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
            return true;
        }
    }
}