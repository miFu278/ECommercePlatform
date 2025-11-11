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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            _mapper = mapper;
        }
        public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
        {
            // Check if email exists
            if (await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken) != null)
            {
                throw new BusinessException("Email already registered", "EMAIL_EXISTS");
            }

            // Check if username exists (if provided)
            if (!string.IsNullOrEmpty(dto.Username))
            {
                if (await _unitOfWork.Users.GetByUsernameAsync(dto.Username, cancellationToken) != null)
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

            // Add user role BEFORE adding to database
            user.UserRoles.Add(new Domain.Entities.UserRole
            {
                UserId = user.Id,
                RoleId = 3, // Customer role
                AssignedAt = DateTime.UtcNow
            });

            // Create user in database (with role)
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload user with roles to ensure navigation properties are loaded
            var createdUser = await _unitOfWork.Users.GetByIdAsync(user.Id, cancellationToken);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken, cancellationToken);

            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            // Get user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            // Check if account is locked and auto-unlock if lockout period has expired
            if (user.IsLocked)
            {
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTime.UtcNow)
                {
                    // Lockout period has expired, auto-unlock the account
                    user.UnlockAccount();
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // Still locked, calculate remaining time
                    var remainingTime = user.LockoutEnd.HasValue
                        ? user.LockoutEnd.Value - DateTime.UtcNow
                        : TimeSpan.Zero;
                    throw new BusinessException(
                        $"Account is locked. Try again in {Math.Ceiling(remainingTime.TotalMinutes)} minutes.",
                        "ACCOUNT_LOCKED");
                }
            }
            // Verify password
            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.IncrementFailedLoginAttempts(
                    AppConstants.Security.MaxFailedLoginAttempts,
                    AppConstants.Security.LockoutMinutes);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedException("Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
                throw new BusinessException("Account is inactive", "ACCOUNT_INACTIVE");

            // Reset failed attempts on successful login
            user.ResetFailedLoginAttempts();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken, cancellationToken);

            if (user == null)
                throw new UnauthorizedException("Invalid refresh token");

            var session = user.Sessions.FirstOrDefault(s => s.RefreshToken == refreshToken && s.IsValid);
            
            if (session == null)
                throw new UnauthorizedException("Invalid or expired refresh token");

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update session
            session.RefreshToken = newRefreshToken;
            session.ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Security.RefreshTokenExpirationDays);
            session.LastAccessedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken, cancellationToken);

            if (user == null)
                return false;

            var session = user.Sessions.FirstOrDefault(s => s.RefreshToken == refreshToken);
            
            if (session == null)
                return false;
                
            session.IsActive = false;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
        {
            // Find user by verification token
            var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(token, cancellationToken);

            if (user == null)
                throw new BusinessException("Invalid verification token", "INVALID_TOKEN");

            // Check if token is expired
            if (!user.IsEmailVerificationTokenValid)
                throw new BusinessException("Verification token has expired", "TOKEN_EXPIRED");

            // Verify email
            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

            if (user == null)
            {
                // Don't reveal if email exists or not for security
                return true;
            }

            // Generate password reset token
            user.PasswordResetToken = _tokenService.GeneratePasswordResetToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send password reset email
            await _emailService.SendPasswordResetAsync(user.Email, user.PasswordResetToken, cancellationToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

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

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ResendEmailVerificationAsync(string email, CancellationToken cancellationToken = default)
        {
            // Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

            if (user == null)
            {
                // Don't reveal if email exists or not for security
                return true;
            }

            // Check if already verified
            if (user.EmailVerified)
                throw new BusinessException("Email is already verified", "EMAIL_ALREADY_VERIFIED");

            // Generate new verification token
            user.EmailVerificationToken = _tokenService.GenerateEmailVerificationToken();
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken, cancellationToken);

            return true;
        }
    }
}