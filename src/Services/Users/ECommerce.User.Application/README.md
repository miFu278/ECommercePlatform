# ECommerce.User.Application

Application layer cho User Service - chứa business logic, DTOs, validators, và services.

## Structure

```
ECommerce.User.Application/
├── DTOs/
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   ├── UserDto.cs
│   ├── LoginResponseDto.cs
│   ├── UpdateProfileDto.cs
│   └── ChangePasswordDto.cs
├── Services/
│   └── UserService.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IUserService.cs
│   ├── IPasswordHasher.cs
│   └── ITokenService.cs
├── Validators/
│   ├── RegisterDtoValidator.cs
│   └── LoginDtoValidator.cs
└── Mappings/
    └── UserMappingProfile.cs
```

## DTOs (Data Transfer Objects)

### RegisterDto
User registration request
```csharp
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "1990-01-15"
}
```

### LoginDto
User login request
```csharp
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

### UserDto
User response
```csharp
{
  "id": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "roles": ["Customer"],
  "emailVerified": true,
  "isActive": true,
  "createdAt": "2025-11-02T10:00:00Z"
}
```

### LoginResponseDto
Login response with tokens
```csharp
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": { UserDto }
}
```

## Validators (FluentValidation)

### RegisterDtoValidator
Validates registration input:
- ✅ Email: Required, valid format, max 255 chars
- ✅ Password: Min 8 chars, uppercase, lowercase, number, special char
- ✅ ConfirmPassword: Must match password
- ✅ FirstName/LastName: Required, max 100 chars
- ✅ PhoneNumber: Valid format (optional)
- ✅ DateOfBirth: Must be in past (optional)

### LoginDtoValidator
Validates login input:
- ✅ Email: Required, valid format
- ✅ Password: Required

## Services

### IUserService
User management operations
```csharp
Task<UserDto?> GetByIdAsync(Guid id);
Task<UserDto?> GetByEmailAsync(string email);
Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
Task<bool> DeleteAccountAsync(Guid userId);
```

### IAuthService
Authentication operations (to be implemented)
```csharp
Task<UserDto> RegisterAsync(RegisterDto dto);
Task<LoginResponseDto> LoginAsync(LoginDto dto);
Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
Task<bool> LogoutAsync(string refreshToken);
Task<bool> VerifyEmailAsync(string token);
Task<bool> ForgotPasswordAsync(string email);
Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
```

### IPasswordHasher
Password hashing (implementation in Infrastructure)
```csharp
string HashPassword(string password);
bool VerifyPassword(string password, string passwordHash);
```

### ITokenService
JWT token generation (implementation in Infrastructure)
```csharp
string GenerateAccessToken(User user);
string GenerateRefreshToken();
Guid? ValidateToken(string token);
```

## AutoMapper Mappings

### UserMappingProfile
- User → UserDto (includes FullName and Roles)
- RegisterDto → User
- UpdateProfileDto → User

## Dependencies

- **FluentValidation** (12.0.0) - Input validation
- **AutoMapper** (15.1.0) - Object mapping
- **ECommerce.Common** - Shared exceptions and models
- **ECommerce.User.Domain** - Domain entities

## Usage Example

### Register User
```csharp
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _validator;
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        // Register
        var user = await _authService.RegisterAsync(dto);
        
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Registration successful"));
    }
}
```

### Update Profile
```csharp
[HttpPut("profile")]
[Authorize]
public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
{
    var userId = User.GetUserId(); // From claims
    var user = await _userService.UpdateProfileAsync(userId, dto);
    
    return Ok(ApiResponse<UserDto>.SuccessResponse(user));
}
```

### Change Password
```csharp
[HttpPost("change-password")]
[Authorize]
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
{
    var userId = User.GetUserId();
    await _userService.ChangePasswordAsync(userId, dto);
    
    return Ok(ApiResponse.SuccessResult("Password changed successfully"));
}
```

## Validation Examples

### Valid Registration
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Invalid Registration (will fail validation)
```json
{
  "email": "invalid-email",           // ❌ Invalid format
  "password": "weak",                 // ❌ Too short, no uppercase/number/special
  "confirmPassword": "different",     // ❌ Doesn't match
  "firstName": "",                    // ❌ Required
  "lastName": ""                      // ❌ Required
}
```

## Error Handling

Services throw custom exceptions from ECommerce.Common:
- `NotFoundException` - When user not found
- `BusinessException` - Business rule violations
- `ValidationException` - Input validation errors

Example:
```csharp
try
{
    await _userService.ChangePasswordAsync(userId, dto);
}
catch (NotFoundException ex)
{
    return NotFound(new { message = ex.Message });
}
catch (BusinessException ex)
{
    return BadRequest(new { message = ex.Message });
}
```

---

**Status**: ✅ Complete  
**Build Status**: ✅ Builds successfully  
**Next Step**: Implement AuthService and Infrastructure services (PasswordHasher, TokenService)
