# User Service API - Email Verification & Password Reset

## Tổng quan

Service này cung cấp các chức năng xác thực người dùng bao gồm:
- Đăng ký tài khoản với email verification
- Đăng nhập/Đăng xuất
- Xác thực email
- Quên mật khẩu
- Đặt lại mật khẩu
- Refresh token

## Cấu hình

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerceUserDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "ECommerceUserService",
    "Audience": "ECommerceClient",
    "ExpirationMinutes": "60"
  },
  "App": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

## API Endpoints

### 1. Đăng ký (Register)
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+84123456789",
  "dateOfBirth": "1990-01-01"
}
```

**Response:**
```json
{
  "message": "Registration successful. Please check your email to verify your account.",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "emailVerified": false
  }
}
```

**Lưu ý:** Sau khi đăng ký, một email verification token sẽ được gửi đến email của người dùng.

### 2. Xác thực Email (Verify Email)
```http
GET /api/auth/verify-email?token={verification_token}
```

**Response:**
```json
{
  "message": "Email verified successfully"
}
```

### 3. Đăng nhập (Login)
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
  "accessToken": "jwt_token",
  "refreshToken": "refresh_token",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "emailVerified": true
  }
}
```

### 4. Quên mật khẩu (Forgot Password)
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "If the email exists, a password reset link has been sent"
}
```

**Lưu ý:** Một password reset token sẽ được gửi đến email (có hiệu lực trong 1 giờ).

### 5. Đặt lại mật khẩu (Reset Password)
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

**Response:**
```json
{
  "message": "Password reset successfully"
}
```

**Lưu ý:** Sau khi reset password thành công, tất cả các session hiện tại sẽ bị vô hiệu hóa.

### 6. Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "current_refresh_token"
}
```

**Response:**
```json
{
  "accessToken": "new_jwt_token",
  "refreshToken": "new_refresh_token",
  "expiresIn": 3600,
  "user": { ... }
}
```

### 7. Đăng xuất (Logout)
```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "current_refresh_token"
}
```

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

## Quy trình hoạt động

### Email Verification Flow
1. User đăng ký tài khoản → `POST /api/auth/register`
2. System tạo `EmailVerificationToken` (có hiệu lực 24 giờ)
3. System gửi email chứa link verification
4. User click vào link → `GET /api/auth/verify-email?token={token}`
5. System xác thực token và đánh dấu email đã được verify

### Password Reset Flow
1. User quên mật khẩu → `POST /api/auth/forgot-password`
2. System tạo `PasswordResetToken` (có hiệu lực 1 giờ)
3. System gửi email chứa link reset password
4. User click vào link và nhập mật khẩu mới → `POST /api/auth/reset-password`
5. System xác thực token, cập nhật mật khẩu và vô hiệu hóa tất cả sessions

## Validation Rules

### Password Requirements
- Tối thiểu 8 ký tự
- Ít nhất 1 chữ hoa
- Ít nhất 1 chữ thường
- Ít nhất 1 số
- Ít nhất 1 ký tự đặc biệt

### Email Requirements
- Định dạng email hợp lệ
- Tối đa 255 ký tự

## Error Codes

| Code | Description |
|------|-------------|
| EMAIL_EXISTS | Email đã được đăng ký |
| INVALID_TOKEN | Token không hợp lệ |
| TOKEN_EXPIRED | Token đã hết hạn |
| ACCOUNT_LOCKED | Tài khoản bị khóa |
| ACCOUNT_INACTIVE | Tài khoản không hoạt động |
| VALIDATION_ERROR | Lỗi validation dữ liệu |

## Security Features

1. **Password Hashing**: Sử dụng BCrypt để hash password
2. **JWT Authentication**: Access token có thời hạn 60 phút
3. **Refresh Token**: Có thời hạn 7 ngày
4. **Account Lockout**: Khóa tài khoản sau 5 lần đăng nhập sai
5. **Token Expiration**: Email verification token (24h), Password reset token (1h)
6. **Session Management**: Vô hiệu hóa tất cả sessions khi reset password

## Email Service

Hiện tại EmailService chỉ log thông tin. Để triển khai thực tế, bạn cần:

1. Cấu hình SMTP hoặc sử dụng dịch vụ như SendGrid, AWS SES
2. Cập nhật `EmailService.cs` để gửi email thực sự
3. Tạo email templates cho verification và password reset

## Testing

Sử dụng file `Auth.http` để test các endpoints:

```bash
# Chạy API
dotnet run

# Sử dụng REST Client extension trong VS Code để test
```

## Migration

Chạy migration để tạo database:

```bash
cd src/Services/Users/ECommerce.User.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../ECommerce.User.API
dotnet ef database update --startup-project ../ECommerce.User.API
```
