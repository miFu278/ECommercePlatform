# Swagger API Documentation Guide

## Overview

User Service API sử dụng Swagger/OpenAPI để tự động generate interactive API documentation.

## Accessing Swagger UI

### Development Environment

Khi chạy API trong Development mode:

```bash
cd src/Services/Users/ECommerce.User.API
dotnet run
```

Swagger UI sẽ available tại:
- **URL:** http://localhost:5000
- **Swagger JSON:** http://localhost:5000/swagger/v1/swagger.json

### Features

Swagger UI cung cấp:
- 📖 **Interactive Documentation** - Xem tất cả endpoints và schemas
- 🧪 **Try It Out** - Test API trực tiếp từ browser
- 🔐 **Authentication** - Test với JWT tokens
- 📝 **Request/Response Examples** - Xem examples cho mỗi endpoint
- 📊 **Schema Definitions** - Xem DTOs và models

## Using Swagger UI

### 1. View API Endpoints

Swagger UI hiển thị tất cả endpoints được group theo controllers:
- **Auth** - Authentication endpoints
- **User** - Profile management endpoints

### 2. Test Endpoints

#### Without Authentication

Endpoints không cần authentication (public):
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/verify-email
- POST /api/auth/forgot-password
- POST /api/auth/reset-password

**Steps:**
1. Click vào endpoint
2. Click "Try it out"
3. Nhập request body
4. Click "Execute"
5. Xem response

#### With Authentication

Endpoints cần authentication:
- GET /api/user/profile
- PUT /api/user/profile
- POST /api/user/change-password
- DELETE /api/user/account

**Steps:**
1. **Login để lấy token:**
   - Expand POST /api/auth/login
   - Click "Try it out"
   - Nhập credentials:
     ```json
     {
       "email": "user@example.com",
       "password": "Password@123"
     }
     ```
   - Click "Execute"
   - Copy `accessToken` từ response

2. **Authorize Swagger:**
   - Click button "Authorize" 🔓 ở góc trên bên phải
   - Nhập: `Bearer {your_access_token}`
   - Click "Authorize"
   - Click "Close"

3. **Test authenticated endpoints:**
   - Bây giờ có thể test các endpoints cần authentication
   - Token sẽ tự động được thêm vào header

### 3. View Schemas

Scroll xuống dưới cùng để xem:
- **Request DTOs** - RegisterDto, LoginDto, UpdateProfileDto, etc.
- **Response DTOs** - UserDto, LoginResponseDto, etc.
- **Field descriptions** - Type, required/optional, validation rules

## API Endpoints Documentation

### Authentication Endpoints

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+84123456789",
  "dateOfBirth": "1990-01-01"
}
```

**Response:** 200 OK
```json
{
  "message": "Registration successful. Please check your email to verify your account.",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    ...
  }
}
```

#### POST /api/auth/login
Login with email and password.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password@123"
}
```

**Response:** 200 OK
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 3600,
  "user": { ... }
}
```

#### GET /api/auth/verify-email
Verify email address with token.

**Query Parameters:**
- `token` (required) - Email verification token

**Response:** 200 OK
```json
{
  "message": "Email verified successfully"
}
```

#### POST /api/auth/forgot-password
Request password reset email.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:** 200 OK
```json
{
  "message": "If the email exists, a password reset link has been sent"
}
```

#### POST /api/auth/reset-password
Reset password with token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "token": "reset_token",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

**Response:** 200 OK
```json
{
  "message": "Password reset successfully"
}
```

#### POST /api/auth/refresh-token
Refresh access token.

**Request Body:**
```json
{
  "refreshToken": "your_refresh_token"
}
```

**Response:** 200 OK - Same as login response

#### POST /api/auth/logout
Logout and invalidate refresh token.

**Request Body:**
```json
{
  "refreshToken": "your_refresh_token"
}
```

**Response:** 200 OK
```json
{
  "message": "Logged out successfully"
}
```

### Profile Management Endpoints

All endpoints require authentication (Bearer token).

#### GET /api/user/profile
Get current user profile.

**Response:** 200 OK
```json
{
  "id": "guid",
  "email": "user@example.com",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "phoneNumber": "+84123456789",
  "dateOfBirth": "1990-01-01",
  "emailVerified": true,
  "isActive": true,
  "roles": ["Customer"],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

#### PUT /api/user/profile
Update current user profile.

**Request Body:**
```json
{
  "username": "johndoe_updated",
  "firstName": "John",
  "lastName": "Doe Updated",
  "phoneNumber": "+84987654321",
  "dateOfBirth": "1990-05-15"
}
```

**Response:** 200 OK - Updated user object

#### POST /api/user/change-password
Change password.

**Request Body:**
```json
{
  "currentPassword": "OldPassword@123",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

**Response:** 200 OK
```json
{
  "message": "Password changed successfully. Please login again."
}
```

#### DELETE /api/user/account
Delete account (soft delete).

**Request Body:**
```json
{
  "password": "CurrentPassword@123"
}
```

**Response:** 200 OK
```json
{
  "message": "Account deleted successfully"
}
```

### Admin Endpoints

Require Admin role.

#### GET /api/user/{id}
Get user by ID.

**Path Parameters:**
- `id` (guid) - User ID

**Response:** 200 OK - User object

#### GET /api/user
Get all users (paginated).

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 10) - Items per page

**Response:** 200 OK - Array of user objects

## Response Codes

### Success Codes
- **200 OK** - Request successful
- **201 Created** - Resource created

### Client Error Codes
- **400 Bad Request** - Invalid input or validation error
- **401 Unauthorized** - Missing or invalid authentication
- **403 Forbidden** - Insufficient permissions
- **404 Not Found** - Resource not found

### Server Error Codes
- **500 Internal Server Error** - Server error

## Error Response Format

```json
{
  "message": "Error description",
  "code": "ERROR_CODE",
  "errors": [
    {
      "field": "fieldName",
      "message": "Field error message"
    }
  ]
}
```

## Swagger Configuration

### Location
Configuration in `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce User Service API",
        Version = "v1",
        Description = "API for user authentication and profile management"
    });
    
    // JWT Authentication
    options.AddSecurityDefinition("Bearer", ...);
    options.AddSecurityRequirement(...);
    
    // XML Comments
    options.IncludeXmlComments(xmlPath);
});
```

### XML Documentation
Enabled in `ECommerce.User.API.csproj`:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

## Production Considerations

### Disable Swagger in Production

Update `Program.cs`:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Secure Swagger in Production

If you need Swagger in production:

```csharp
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api-docs"; // Change from root
});

// Add authentication middleware before Swagger
app.UseAuthentication();
app.UseAuthorization();
```

### Alternative: Export OpenAPI Spec

Export to file for external documentation:

```bash
# Install tool
dotnet tool install --global Microsoft.dotnet-openapi

# Generate spec
dotnet run --project ECommerce.User.API
curl http://localhost:5000/swagger/v1/swagger.json > openapi.json
```

## Best Practices

### For API Developers

1. **Add XML Comments** - Document all public endpoints
2. **Use ProducesResponseType** - Specify response types
3. **Add Examples** - Provide request/response examples
4. **Group Endpoints** - Use controllers to organize
5. **Version API** - Use versioning for breaking changes

### For API Consumers

1. **Read Documentation** - Understand endpoints before using
2. **Test in Swagger** - Verify requests before implementing
3. **Handle Errors** - Check all possible response codes
4. **Use Schemas** - Reference DTOs for request/response structure
5. **Save Examples** - Export working requests for reference

## Troubleshooting

### Swagger UI Not Loading

**Check:**
1. API is running
2. Navigate to http://localhost:5000
3. Check browser console for errors
4. Verify Swagger is enabled in Development

### Authentication Not Working

**Steps:**
1. Login to get token
2. Click "Authorize" button
3. Enter: `Bearer {token}` (with space)
4. Click "Authorize"
5. Try authenticated endpoint

### XML Comments Not Showing

**Check:**
1. `GenerateDocumentationFile` is true in csproj
2. XML file exists in bin folder
3. Path is correct in Program.cs
4. Rebuild project

---

**Access Swagger:** http://localhost:5000 (when API is running)
**Last Updated:** November 2, 2025
