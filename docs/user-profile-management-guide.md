# User Profile Management Guide

## Overview

User Profile Management cho phép users quản lý thông tin cá nhân, đổi mật khẩu và xóa tài khoản.

## Features

### 1. Get Profile
Lấy thông tin profile của user hiện tại.

**Endpoint:** `GET /api/user/profile`

**Authorization:** Required (Bearer Token)

**Response:**
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

### 2. Update Profile
Cập nhật thông tin profile.

**Endpoint:** `PUT /api/user/profile`

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe Updated",
  "phoneNumber": "+84987654321",
  "dateOfBirth": "1990-05-15"
}
```

**Validation Rules:**
- `firstName`: Required, max 100 characters
- `lastName`: Required, max 100 characters
- `phoneNumber`: Optional, must be valid phone format
- `dateOfBirth`: Optional, must be in the past

**Response:**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe Updated",
  ...
}
```

### 3. Change Password
Đổi mật khẩu.

**Endpoint:** `POST /api/user/change-password`

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "OldPassword@123",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character
- Must be different from current password

**Response:**
```json
{
  "message": "Password changed successfully. Please login again."
}
```

**Note:** Sau khi đổi password, tất cả sessions sẽ bị logout (security measure).

### 4. Delete Account
Xóa tài khoản (soft delete).

**Endpoint:** `DELETE /api/user/account`

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "password": "CurrentPassword@123"
}
```

**Response:**
```json
{
  "message": "Account deleted successfully"
}
```

**Note:** 
- Soft delete (data vẫn còn trong database)
- Cần verify password trước khi xóa
- Account không thể login sau khi xóa

### 5. Get User By ID (Admin Only)
Lấy thông tin user theo ID.

**Endpoint:** `GET /api/user/{id}`

**Authorization:** Required (Bearer Token + Admin Role)

**Response:** Same as Get Profile

### 6. Get All Users (Admin Only)
Lấy danh sách tất cả users (phân trang).

**Endpoint:** `GET /api/user?pageNumber=1&pageSize=10`

**Authorization:** Required (Bearer Token + Admin Role)

**Query Parameters:**
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

**Response:**
```json
[
  {
    "id": "guid",
    "email": "user1@example.com",
    ...
  },
  {
    "id": "guid",
    "email": "user2@example.com",
    ...
  }
]
```

## Authentication Flow

### 1. Login to Get Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password@123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 3600,
  "user": { ... }
}
```

### 2. Use Token in Requests
```http
GET /api/user/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Error Responses

### 401 Unauthorized
```json
{
  "message": "Unauthorized",
  "code": "UNAUTHORIZED"
}
```

### 403 Forbidden
```json
{
  "message": "Forbidden",
  "code": "FORBIDDEN"
}
```

### 404 Not Found
```json
{
  "message": "User not found",
  "code": "NOT_FOUND"
}
```

### 400 Bad Request (Validation)
```json
{
  "message": "Validation failed",
  "code": "VALIDATION_ERROR",
  "errors": [
    {
      "field": "firstName",
      "message": "First name is required"
    }
  ]
}
```

### 400 Bad Request (Business Logic)
```json
{
  "message": "Current password is incorrect",
  "code": "INVALID_PASSWORD"
}
```

## Security Features

### 1. JWT Authentication
- All endpoints require valid JWT token
- Token contains user ID and roles
- Token expires after 60 minutes

### 2. Authorization
- Regular users can only access their own profile
- Admin users can access all users

### 3. Password Change Security
- Requires current password verification
- New password must be different
- All sessions invalidated after change

### 4. Account Deletion Security
- Requires password verification
- Soft delete (data preserved)
- Cannot be undone without admin

## Testing

### Test File
Use `user-profile.http` for testing:

```http
@baseUrl = http://localhost:5000
@token = YOUR_ACCESS_TOKEN_HERE

### Get Profile
GET {{baseUrl}}/api/user/profile
Authorization: Bearer {{token}}
```

### Test Flow

1. **Login to get token:**
   ```bash
   POST /api/auth/login
   # Copy accessToken from response
   ```

2. **Get profile:**
   ```bash
   GET /api/user/profile
   Authorization: Bearer {token}
   ```

3. **Update profile:**
   ```bash
   PUT /api/user/profile
   # Update firstName, lastName, etc.
   ```

4. **Change password:**
   ```bash
   POST /api/user/change-password
   # Provide current and new password
   ```

5. **Login with new password:**
   ```bash
   POST /api/auth/login
   # Use new password
   ```

## Best Practices

### For Clients

1. **Store Token Securely**
   - Use secure storage (not localStorage for sensitive apps)
   - Clear token on logout

2. **Handle Token Expiration**
   - Implement token refresh logic
   - Redirect to login on 401

3. **Validate Input Client-Side**
   - Check password requirements before submit
   - Validate phone number format

4. **Confirm Sensitive Actions**
   - Ask for confirmation before delete account
   - Show warning about password change logout

### For Backend

1. **Always Verify User Identity**
   - Use JWT claims to get user ID
   - Never trust user ID from request body

2. **Validate All Input**
   - Use FluentValidation
   - Check business rules

3. **Log Important Actions**
   - Log password changes
   - Log account deletions
   - Log failed attempts

4. **Rate Limiting**
   - Limit password change attempts
   - Limit profile update frequency

## Integration Examples

### React/TypeScript
```typescript
// Get Profile
const getProfile = async () => {
  const token = localStorage.getItem('accessToken');
  const response = await fetch('http://localhost:5000/api/user/profile', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
};

// Update Profile
const updateProfile = async (data: UpdateProfileDto) => {
  const token = localStorage.getItem('accessToken');
  const response = await fetch('http://localhost:5000/api/user/profile', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
  return await response.json();
};
```

### C# Client
```csharp
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

// Get Profile
var response = await client.GetAsync("http://localhost:5000/api/user/profile");
var user = await response.Content.ReadFromJsonAsync<UserDto>();

// Update Profile
var updateDto = new UpdateProfileDto { ... };
var response = await client.PutAsJsonAsync(
    "http://localhost:5000/api/user/profile", 
    updateDto
);
```

## Troubleshooting

### Token Invalid/Expired
- Login again to get new token
- Implement refresh token logic

### 403 Forbidden on Admin Endpoints
- Check user has Admin role
- Verify JWT contains correct role claims

### Password Change Fails
- Verify current password is correct
- Check new password meets requirements
- Ensure new password is different

### Profile Update Not Reflected
- Check response for updated data
- Reload profile after update
- Clear any client-side cache

---

**Status:** ✅ Complete and ready to use
**Last Updated:** November 2, 2025
