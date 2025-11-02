# Hướng dẫn Email Verification và Password Reset

## Tổng quan

Tài liệu này mô tả chi tiết về các chức năng xác thực email và đặt lại mật khẩu đã được triển khai trong User Service.

## Các chức năng đã hoàn thiện

### 1. Email Verification (Xác thực Email)

**Mục đích:** Xác minh địa chỉ email của người dùng sau khi đăng ký.

**Quy trình:**
1. Người dùng đăng ký tài khoản mới
2. Hệ thống tạo một token xác thực email (có hiệu lực 24 giờ)
3. Token được lưu vào database (trường `EmailVerificationToken` và `EmailVerificationTokenExpires`)
4. Hệ thống gửi email chứa link xác thực đến người dùng
5. Người dùng click vào link để xác thực email
6. Hệ thống kiểm tra token và đánh dấu email đã được xác thực

**Endpoint:**
```
GET /api/auth/verify-email?token={verification_token}
```

**Các trường liên quan trong User entity:**
- `EmailVerified`: boolean - Trạng thái xác thực email
- `EmailVerificationToken`: string - Token xác thực
- `EmailVerificationTokenExpires`: DateTime - Thời gian hết hạn token

### 2. Forgot Password (Quên mật khẩu)

**Mục đích:** Cho phép người dùng yêu cầu đặt lại mật khẩu khi quên.

**Quy trình:**
1. Người dùng nhập email để yêu cầu đặt lại mật khẩu
2. Hệ thống kiểm tra email có tồn tại không
3. Tạo token đặt lại mật khẩu (có hiệu lực 1 giờ)
4. Token được lưu vào database (trường `PasswordResetToken` và `PasswordResetTokenExpires`)
5. Gửi email chứa link đặt lại mật khẩu
6. Trả về thông báo thành công (không tiết lộ email có tồn tại hay không vì lý do bảo mật)

**Endpoint:**
```
POST /api/auth/forgot-password
Body: { "email": "user@example.com" }
```

**Các trường liên quan trong User entity:**
- `PasswordResetToken`: string - Token đặt lại mật khẩu
- `PasswordResetTokenExpires`: DateTime - Thời gian hết hạn token

### 3. Reset Password (Đặt lại mật khẩu)

**Mục đích:** Cho phép người dùng đặt mật khẩu mới sau khi xác thực token.

**Quy trình:**
1. Người dùng nhập email, token và mật khẩu mới
2. Hệ thống xác thực token
3. Kiểm tra token có hết hạn không
4. Hash mật khẩu mới
5. Cập nhật mật khẩu và xóa token
6. Vô hiệu hóa tất cả các session hiện tại (bắt buộc đăng nhập lại)

**Endpoint:**
```
POST /api/auth/reset-password
Body: {
  "email": "user@example.com",
  "token": "reset_token",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

## Cấu trúc Code

### 1. DTOs (Data Transfer Objects)

**ForgotPasswordDto.cs**
```csharp
public class ForgotPasswordDto
{
    public string Email { get; set; }
}
```

**ResetPasswordDto.cs**
```csharp
public class ResetPasswordDto
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
```

### 2. Services

**IEmailService.cs** - Interface cho email service
```csharp
public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default);
}
```

**EmailService.cs** - Implementation (hiện tại chỉ log, cần cấu hình SMTP thực tế)
- Gửi email xác thực
- Gửi email đặt lại mật khẩu

**ITokenService.cs** - Thêm methods tạo token
```csharp
string GenerateEmailVerificationToken();
string GeneratePasswordResetToken();
```

**TokenService.cs** - Implementation
- Tạo token ngẫu nhiên 32 bytes
- Encode base64 và làm URL-safe

### 3. AuthService Implementation

**VerifyEmailAsync**
```csharp
public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
{
    // Tìm user theo token
    // Kiểm tra token có hợp lệ và chưa hết hạn
    // Đánh dấu email đã xác thực
    // Xóa token
}
```

**ForgotPasswordAsync**
```csharp
public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
{
    // Tìm user theo email
    // Tạo password reset token
    // Lưu token và thời gian hết hạn
    // Gửi email
}
```

**ResetPasswordAsync**
```csharp
public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
{
    // Tìm user theo email
    // Xác thực token
    // Hash mật khẩu mới
    // Cập nhật password
    // Xóa token
    // Vô hiệu hóa tất cả sessions
}
```

### 4. Controller

**AuthController.cs** - Các endpoints mới:
- `GET /api/auth/verify-email`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

### 5. Validators

**ForgotPasswordDtoValidator.cs**
- Validate email format

**ResetPasswordDtoValidator.cs**
- Validate email, token, password
- Kiểm tra password requirements
- Kiểm tra password và confirm password khớp nhau

## Bảo mật

### 1. Token Security
- Email verification token: 24 giờ
- Password reset token: 1 giờ
- Token được tạo ngẫu nhiên 32 bytes
- URL-safe encoding

### 2. Password Requirements
- Tối thiểu 8 ký tự
- Ít nhất 1 chữ hoa
- Ít nhất 1 chữ thường
- Ít nhất 1 số
- Ít nhất 1 ký tự đặc biệt

### 3. Session Management
- Khi reset password, tất cả sessions hiện tại bị vô hiệu hóa
- Người dùng phải đăng nhập lại

### 4. Information Disclosure Prevention
- Forgot password không tiết lộ email có tồn tại hay không
- Luôn trả về thông báo thành công

## Cấu hình

### appsettings.json
```json
{
  "App": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

BaseUrl được sử dụng để tạo link trong email.

## Testing

### 1. Test Email Verification

```http
# 1. Đăng ký user mới
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123456",
  "confirmPassword": "Test@123456",
  "firstName": "Test",
  "lastName": "User"
}

# 2. Lấy token từ database hoặc log
# 3. Verify email
GET http://localhost:5000/api/auth/verify-email?token={token}
```

### 2. Test Password Reset

```http
# 1. Request password reset
POST http://localhost:5000/api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}

# 2. Lấy token từ database hoặc log
# 3. Reset password
POST http://localhost:5000/api/auth/reset-password
Content-Type: application/json

{
  "email": "test@example.com",
  "token": "{token}",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}

# 4. Login với password mới
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "NewPassword@123"
}
```

## Triển khai Email Service thực tế

Hiện tại EmailService chỉ log thông tin. Để triển khai thực tế:

### Option 1: SMTP
```csharp
using System.Net;
using System.Net.Mail;

public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
{
    var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
    
    using var client = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]))
    {
        Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
        EnableSsl = true
    };
    
    var message = new MailMessage
    {
        From = new MailAddress(_configuration["Smtp:FromEmail"]),
        Subject = "Verify your email",
        Body = $"Please click the link to verify your email: {verificationUrl}",
        IsBodyHtml = true
    };
    message.To.Add(email);
    
    await client.SendMailAsync(message, cancellationToken);
}
```

### Option 2: SendGrid
```csharp
using SendGrid;
using SendGrid.Helpers.Mail;

public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
{
    var apiKey = _configuration["SendGrid:ApiKey"];
    var client = new SendGridClient(apiKey);
    
    var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
    
    var msg = new SendGridMessage
    {
        From = new EmailAddress(_configuration["SendGrid:FromEmail"], "ECommerce Platform"),
        Subject = "Verify your email",
        HtmlContent = $"<p>Please click the link to verify your email: <a href='{verificationUrl}'>Verify Email</a></p>"
    };
    msg.AddTo(new EmailAddress(email));
    
    await client.SendEmailAsync(msg, cancellationToken);
}
```

### Cấu hình appsettings.json

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@ecommerce.com"
  },
  "SendGrid": {
    "ApiKey": "your-sendgrid-api-key",
    "FromEmail": "noreply@ecommerce.com"
  }
}
```

## Error Handling

Các lỗi có thể xảy ra:

| Error Code | Description | HTTP Status |
|------------|-------------|-------------|
| INVALID_TOKEN | Token không hợp lệ | 400 |
| TOKEN_EXPIRED | Token đã hết hạn | 400 |
| INVALID_REQUEST | Request không hợp lệ | 400 |
| VALIDATION_ERROR | Lỗi validation | 400 |

## Database Schema

Các trường đã có sẵn trong User entity:

```sql
-- Email Verification
EmailVerified BIT NOT NULL DEFAULT 0,
EmailVerificationToken NVARCHAR(255) NULL,
EmailVerificationTokenExpires DATETIME2 NULL,

-- Password Reset
PasswordResetToken NVARCHAR(255) NULL,
PasswordResetTokenExpires DATETIME2 NULL
```

## Checklist hoàn thành

- [x] Tạo DTOs (ForgotPasswordDto, ResetPasswordDto)
- [x] Tạo IEmailService interface
- [x] Implement EmailService (với logging)
- [x] Thêm methods vào ITokenService
- [x] Implement token generation trong TokenService
- [x] Implement VerifyEmailAsync trong AuthService
- [x] Implement ForgotPasswordAsync trong AuthService
- [x] Implement ResetPasswordAsync trong AuthService
- [x] Tạo AuthController với các endpoints mới
- [x] Tạo Validators cho DTOs mới
- [x] Cấu hình DI trong Program.cs
- [x] Tạo ExceptionHandlingMiddleware
- [x] Tạo file test Auth.http
- [x] Cập nhật appsettings.json
- [x] Viết documentation

## Các bước tiếp theo

1. **Triển khai Email Service thực tế**
   - Chọn provider (SMTP, SendGrid, AWS SES, etc.)
   - Tạo email templates đẹp
   - Test gửi email thực tế

2. **Tạo Migration và Update Database**
   ```bash
   cd src/Services/Users/ECommerce.User.Infrastructure
   dotnet ef migrations add AddEmailVerificationAndPasswordReset --startup-project ../ECommerce.User.API
   dotnet ef database update --startup-project ../ECommerce.User.API
   ```

3. **Testing**
   - Unit tests cho AuthService
   - Integration tests cho API endpoints
   - Test email sending

4. **Frontend Integration**
   - Tạo trang verify email
   - Tạo trang reset password
   - Xử lý các trường hợp lỗi

5. **Monitoring & Logging**
   - Log các sự kiện quan trọng
   - Monitor email delivery rate
   - Alert khi có lỗi

## Lưu ý quan trọng

1. **Security:**
   - Không bao giờ log token trong production
   - Sử dụng HTTPS cho tất cả endpoints
   - Rate limiting cho forgot password endpoint

2. **User Experience:**
   - Email nên được gửi nhanh chóng
   - Cung cấp link resend verification email
   - Thông báo rõ ràng khi token hết hạn

3. **Production:**
   - Sử dụng queue (RabbitMQ, Azure Service Bus) để gửi email async
   - Implement retry logic cho email sending
   - Monitor email bounce rate
