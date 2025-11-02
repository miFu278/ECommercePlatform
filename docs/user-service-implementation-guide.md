# User Service Implementation Guide

Hướng dẫn chi tiết để hoàn thiện User Service API và test.

---

## 📋 Checklist

- [x] Domain Layer (Entities, Interfaces)
- [x] Infrastructure Layer (DbContext, Repositories, PasswordHasher, TokenService)
- [x] Application Layer (DTOs, Services, Validators, Mappings)
- [ ] AuthService Implementation
- [ ] API Layer (Controllers, Program.cs)
- [ ] Database Migration
- [ ] Testing

---

## Step 1: Implement AuthService

### File: `src/Services/Users/ECommerce.User.Application/Services/AuthService.cs`

```csharp
using AutoMapper;
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Domain.Interfaces;
using ECommerce.Common.Exceptions;
using ECommerce.Common.Constants;

namespace ECommerce.User.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(dto.Email, cancellationToken))
            throw new BusinessException("Email already registered", "EMAIL_EXISTS");

        // Create user entity
        var user = _mapper.Map<Domain.Entities.User>(dto);
        user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
        user.EmailVerified = false;
        user.IsActive = true;

        // Create user
        await _userRepository.CreateAsync(user, cancellationToken);

        // Assign Customer role (default)
        user.UserRoles.Add(new Domain.Entities.UserRole
        {
            UserId = user.Id,
            RoleId = 3, // Customer role
            AssignedAt = DateTime.UtcNow
        });

        await _userRepository.UpdateAsync(user, cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null)
            throw new UnauthorizedException("Invalid email or password");

        // Check if account is locked
        if (user.IsLockedOut)
        {
            var remainingTime = user.LockoutEnd!.Value - DateTime.UtcNow;
            throw new BusinessException(
                $"Account is locked. Try again in {remainingTime.Minutes} minutes",
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
        var session = new Domain.Entities.UserSession
        {
            UserId = user.Id,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(AppConstants.Security.RefreshTokenExpirationDays),
            IsActive = true
        };
        user.Sessions.Add(session);
        await _userRepository.UpdateAsync(user, cancellationToken);

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

        // Generate new tokens
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
        var users = await _userRepository.GetAllAsync(1, 1000, cancellationToken);
        var user = users.FirstOrDefault(u => u.Sessions.Any(s => s.RefreshToken == refreshToken));

        if (user == null)
            return false;

        var session = user.Sessions.First(s => s.RefreshToken == refreshToken);
        session.IsActive = false;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email verification
        throw new NotImplementedException();
    }

    public Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        // TODO: Implement forgot password
        throw new NotImplementedException();
    }

    public Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        // TODO: Implement reset password
        throw new NotImplementedException();
    }
}
```

---

## Step 2: Create Controllers

### File: `src/Services/Users/ECommerce.User.API/Controllers/AuthController.cs`

```csharp
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.User.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Validation failed",
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()));

        var user = await _authService.RegisterAsync(dto);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Registration successful"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Validation failed",
                validationResult.Errors.Select(e => e.ErrorMessage).ToList()));

        var response = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful"));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(ApiResponse.SuccessResult("Logout successful"));
    }
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
```

### File: `src/Services/Users/ECommerce.User.API/Controllers/UsersController.cs`

```csharp
using ECommerce.User.Application.DTOs;
using ECommerce.User.Application.Interfaces;
using ECommerce.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.User.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.UpdateProfileAsync(userId, dto);

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profile updated successfully"));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.ChangePasswordAsync(userId, dto);

        return Ok(ApiResponse.SuccessResult("Password changed successfully"));
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.DeleteAccountAsync(userId);

        return Ok(ApiResponse.SuccessResult("Account deleted successfully"));
    }
}
```

---

## Step 3: Configure Program.cs

### File: `src/Services/Users/ECommerce.User.API/Program.cs`

```csharp
using System.Text;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Application.Mappings;
using ECommerce.User.Application.Services;
using ECommerce.User.Application.Validators;
using ECommerce.User.Domain.Interfaces;
using ECommerce.User.Infrastructure.Data;
using ECommerce.User.Infrastructure.Repositories;
using ECommerce.User.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Service API",
        Version = "v1",
        Description = "E-Commerce User Service API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Step 4: Configure appsettings.json

### File: `src/Services/Users/ECommerce.User.API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "UserDb": "Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-super-secret-jwt-key-must-be-at-least-32-characters-long",
    "Issuer": "ECommerceUserService",
    "Audience": "ECommerceClient",
    "ExpirationMinutes": "60"
  }
}
```

---

## Step 5: Create Migration

```powershell
# Navigate to API project
cd src/Services/Users/ECommerce.User.API

# Create migration
dotnet ef migrations add InitialCreate --project ../ECommerce.User.Infrastructure

# Apply migration
dotnet ef database update --project ../ECommerce.User.Infrastructure
```

---

## Step 6: Run and Test

### Start Docker Infrastructure
```powershell
cd docker
.\start.ps1
```

### Run API
```powershell
cd src/Services/Users/ECommerce.User.API
dotnet run
```

### Test Endpoints

**1. Register User**
```http
POST http://localhost:5000/api/v1/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

**2. Login**
```http
POST http://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!@#"
}
```

**3. Get Profile**
```http
GET http://localhost:5000/api/v1/users/profile
Authorization: Bearer {your_access_token}
```

**4. Update Profile**
```http
PUT http://localhost:5000/api/v1/users/profile
Authorization: Bearer {your_access_token}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "+9876543210"
}
```

---

## 📝 Checklist Before Testing

- [ ] Docker infrastructure running (PostgreSQL)
- [ ] appsettings.json configured with correct connection string
- [ ] JWT Secret configured (min 32 characters)
- [ ] Migration created and applied
- [ ] All services registered in Program.cs
- [ ] Controllers created
- [ ] AuthService implemented

---

## 🐛 Common Issues

### Issue 1: Database connection failed
**Solution**: Check Docker is running and connection string is correct

### Issue 2: JWT Secret error
**Solution**: Ensure JWT:Secret in appsettings.json is at least 32 characters

### Issue 3: Migration error
**Solution**: Delete Migrations folder and recreate migration

### Issue 4: 401 Unauthorized
**Solution**: Check JWT token is included in Authorization header as "Bearer {token}"

---

## 🎯 Next Steps

After User Service works:
1. Add email verification
2. Add password reset
3. Add refresh token rotation
4. Add rate limiting
5. Add logging middleware
6. Add exception handling middleware
7. Move to Product Service

---

**Good luck! 🚀**
