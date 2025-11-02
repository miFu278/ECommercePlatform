# Hướng dẫn Implement Email Service

## Tổng quan

Hiện tại `EmailService.cs` chỉ log thông tin. Guide này sẽ hướng dẫn bạn implement gửi email thực tế với nhiều options khác nhau.

## Options để gửi Email

### Option 1: SMTP (Gmail, Outlook, Custom SMTP Server)
**Ưu điểm:**
- Miễn phí với Gmail/Outlook
- Không cần đăng ký service bên thứ 3
- Dễ setup cho development

**Nhược điểm:**
- Giới hạn số lượng email/ngày
- Có thể bị spam filter
- Không có analytics/tracking

### Option 2: SendGrid
**Ưu điểm:**
- Free tier: 100 emails/day
- Có email templates
- Analytics và tracking
- Deliverability tốt

**Nhược điểm:**
- Cần đăng ký account
- Cần verify domain cho production

### Option 3: AWS SES (Simple Email Service)
**Ưu điểm:**
- Rất rẻ ($0.10 per 1000 emails)
- Scalable
- Tích hợp tốt với AWS ecosystem

**Nhược điểm:**
- Cần AWS account
- Setup phức tạp hơn
- Cần verify domain

### Option 4: Azure Communication Services
**Ưu điểm:**
- Tích hợp tốt với Azure
- Free tier available
- Hỗ trợ nhiều channels (email, SMS, etc.)

**Nhược điểm:**
- Cần Azure subscription
- Cần verify domain

---

## Option 1: SMTP Implementation

### 1.1. Sử dụng Gmail

#### Bước 1: Tạo App Password cho Gmail

1. Đăng nhập Gmail
2. Vào https://myaccount.google.com/security
3. Bật "2-Step Verification"
4. Vào "App passwords"
5. Tạo password mới cho "Mail" application
6. Copy password này (16 ký tự)

#### Bước 2: Cài đặt package (không cần, .NET có built-in)

#### Bước 3: Cập nhật appsettings.json

```json
{
  "Email": {
    "Provider": "SMTP",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-16-char-app-password",
      "FromEmail": "your-email@gmail.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

#### Bước 4: Implement EmailService

```csharp
using System.Net;
using System.Net.Mail;
using ECommerce.User.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.User.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
        
        var subject = "Verify Your Email Address";
        var body = GetEmailVerificationTemplate(verificationUrl);
        
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&email={email}";
        
        var subject = "Reset Your Password";
        var body = GetPasswordResetTemplate(resetUrl);
        
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Email:Smtp:EnableSsl"] ?? "true");
            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            var fromEmail = _configuration["Email:Smtp:FromEmail"];
            var fromName = _configuration["Email:Smtp:FromName"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    private string GetEmailVerificationTemplate(string verificationUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 24px; 
            background-color: #007bff; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0;
        }}
        .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Verify Your Email Address</h2>
        <p>Thank you for registering! Please click the button below to verify your email address:</p>
        <a href='{verificationUrl}' class='button'>Verify Email</a>
        <p>Or copy and paste this link into your browser:</p>
        <p>{verificationUrl}</p>
        <p>This link will expire in 24 hours.</p>
        <div class='footer'>
            <p>If you didn't create an account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate(string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 24px; 
            background-color: #dc3545; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0;
        }}
        .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Reset Your Password</h2>
        <p>We received a request to reset your password. Click the button below to reset it:</p>
        <a href='{resetUrl}' class='button'>Reset Password</a>
        <p>Or copy and paste this link into your browser:</p>
        <p>{resetUrl}</p>
        <p>This link will expire in 1 hour.</p>
        <div class='footer'>
            <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
        </div>
    </div>
</body>
</html>";
    }
}
```

### 1.2. Sử dụng Outlook/Hotmail

Cấu hình tương tự Gmail, chỉ thay đổi:

```json
{
  "Email": {
    "Smtp": {
      "Host": "smtp-mail.outlook.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@outlook.com",
      "Password": "your-password"
    }
  }
}
```

### 1.3. Custom SMTP Server

```json
{
  "Email": {
    "Smtp": {
      "Host": "mail.yourdomain.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "noreply@yourdomain.com",
      "Password": "your-password"
    }
  }
}
```

---

## Option 2: SendGrid Implementation

### Bước 1: Đăng ký SendGrid

1. Vào https://sendgrid.com/
2. Đăng ký free account (100 emails/day)
3. Verify email
4. Tạo API Key:
   - Settings → API Keys → Create API Key
   - Chọn "Full Access" hoặc "Restricted Access" (chỉ Mail Send)
   - Copy API Key

### Bước 2: Cài đặt package

```bash
cd src/Services/Users/ECommerce.User.Infrastructure
dotnet add package SendGrid
```

### Bước 3: Cập nhật appsettings.json

```json
{
  "Email": {
    "Provider": "SendGrid",
    "SendGrid": {
      "ApiKey": "SG.your-api-key-here",
      "FromEmail": "noreply@yourdomain.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

### Bước 4: Implement EmailService

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using ECommerce.User.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.User.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SendGridClient _client;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = _configuration["Email:SendGrid:ApiKey"];
        _client = new SendGridClient(apiKey);
    }

    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
        
        var subject = "Verify Your Email Address";
        var htmlContent = GetEmailVerificationTemplate(verificationUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&email={email}";
        
        var subject = "Reset Your Password";
        var htmlContent = GetPasswordResetTemplate(resetUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        try
        {
            var fromEmail = _configuration["Email:SendGrid:FromEmail"];
            var fromName = _configuration["Email:SendGrid:FromName"];

            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            
            var response = await _client.SendEmailAsync(msg, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Body: {Body}", 
                    toEmail, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    // Templates giống như SMTP version
    private string GetEmailVerificationTemplate(string verificationUrl) { /* ... */ }
    private string GetPasswordResetTemplate(string resetUrl) { /* ... */ }
}
```

### Bước 5: (Optional) Sử dụng SendGrid Templates

SendGrid cho phép tạo templates trên dashboard:

```csharp
public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
{
    var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
    
    var msg = new SendGridMessage();
    msg.SetFrom(new EmailAddress(_configuration["Email:SendGrid:FromEmail"]));
    msg.AddTo(new EmailAddress(email));
    
    // Sử dụng template ID từ SendGrid
    msg.SetTemplateId("d-your-template-id-here");
    
    // Dynamic template data
    msg.SetTemplateData(new
    {
        verification_url = verificationUrl,
        expiry_hours = 24
    });
    
    await _client.SendEmailAsync(msg, cancellationToken);
}
```

---

## Option 3: AWS SES Implementation

### Bước 1: Setup AWS SES

1. Đăng nhập AWS Console
2. Vào SES service
3. Verify email address hoặc domain
4. Tạo SMTP credentials hoặc sử dụng AWS SDK
5. Request production access (mặc định là sandbox mode)

### Bước 2: Cài đặt package

```bash
cd src/Services/Users/ECommerce.User.Infrastructure
dotnet add package AWSSDK.SimpleEmail
```

### Bước 3: Cập nhật appsettings.json

```json
{
  "Email": {
    "Provider": "AWSSES",
    "AWS": {
      "Region": "us-east-1",
      "AccessKeyId": "your-access-key",
      "SecretAccessKey": "your-secret-key",
      "FromEmail": "noreply@yourdomain.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

### Bước 4: Implement EmailService

```csharp
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ECommerce.User.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.User.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IAmazonSimpleEmailService _sesClient;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var region = RegionEndpoint.GetBySystemName(_configuration["Email:AWS:Region"]);
        _sesClient = new AmazonSimpleEmailServiceClient(
            _configuration["Email:AWS:AccessKeyId"],
            _configuration["Email:AWS:SecretAccessKey"],
            region
        );
    }

    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
        
        var subject = "Verify Your Email Address";
        var htmlContent = GetEmailVerificationTemplate(verificationUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&email={email}";
        
        var subject = "Reset Your Password";
        var htmlContent = GetPasswordResetTemplate(resetUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        try
        {
            var fromEmail = _configuration["Email:AWS:FromEmail"];
            var fromName = _configuration["Email:AWS:FromName"];

            var request = new SendEmailRequest
            {
                Source = $"{fromName} <{fromEmail}>",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(htmlContent)
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(request, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", 
                toEmail, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    // Templates giống như các version khác
    private string GetEmailVerificationTemplate(string verificationUrl) { /* ... */ }
    private string GetPasswordResetTemplate(string resetUrl) { /* ... */ }
}
```

---

## Option 4: Azure Communication Services

### Bước 1: Setup Azure Communication Services

1. Tạo Azure Communication Services resource
2. Lấy connection string
3. Setup email domain

### Bước 2: Cài đặt package

```bash
cd src/Services/Users/ECommerce.User.Infrastructure
dotnet add package Azure.Communication.Email
```

### Bước 3: Cập nhật appsettings.json

```json
{
  "Email": {
    "Provider": "AzureCS",
    "Azure": {
      "ConnectionString": "endpoint=https://...;accesskey=...",
      "FromEmail": "noreply@yourdomain.com",
      "FromName": "ECommerce Platform"
    }
  }
}
```

### Bước 4: Implement EmailService

```csharp
using Azure.Communication.Email;
using ECommerce.User.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.User.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailClient _emailClient;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var connectionString = _configuration["Email:Azure:ConnectionString"];
        _emailClient = new EmailClient(connectionString);
    }

    public async Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";
        
        var subject = "Verify Your Email Address";
        var htmlContent = GetEmailVerificationTemplate(verificationUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&email={email}";
        
        var subject = "Reset Your Password";
        var htmlContent = GetPasswordResetTemplate(resetUrl);
        
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        try
        {
            var fromEmail = _configuration["Email:Azure:FromEmail"];
            var fromName = _configuration["Email:Azure:FromName"];

            var emailMessage = new EmailMessage(
                senderAddress: fromEmail,
                recipientAddress: toEmail,
                content: new EmailContent(subject)
                {
                    Html = htmlContent
                }
            );

            var response = await _emailClient.SendAsync(
                Azure.WaitUntil.Started,
                emailMessage,
                cancellationToken
            );
            
            _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", 
                toEmail, response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    // Templates giống như các version khác
    private string GetEmailVerificationTemplate(string verificationUrl) { /* ... */ }
    private string GetPasswordResetTemplate(string resetUrl) { /* ... */ }
}
```

---

## Best Practices

### 1. Email Templates

Tạo file riêng cho templates:

```
src/Services/Users/ECommerce.User.Infrastructure/
  Templates/
    EmailVerification.html
    PasswordReset.html
```

Load templates:

```csharp
private string GetEmailVerificationTemplate(string verificationUrl)
{
    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
        "Templates", "EmailVerification.html");
    var template = File.ReadAllText(templatePath);
    return template.Replace("{{VERIFICATION_URL}}", verificationUrl);
}
```

### 2. Async Queue (Production)

Sử dụng background job để gửi email:

```bash
dotnet add package Hangfire
# hoặc
dotnet add package MassTransit
```

```csharp
// Thay vì gửi trực tiếp
await _emailService.SendEmailVerificationAsync(email, token);

// Sử dụng background job
_backgroundJobClient.Enqueue(() => 
    _emailService.SendEmailVerificationAsync(email, token, CancellationToken.None));
```

### 3. Retry Logic

```csharp
using Polly;

private async Task SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
{
    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    await retryPolicy.ExecuteAsync(async () =>
    {
        // Send email logic
    });
}
```

### 4. Configuration Validation

```csharp
public class EmailOptions
{
    public string Provider { get; set; } = string.Empty;
    public SmtpOptions? Smtp { get; set; }
    public SendGridOptions? SendGrid { get; set; }
}

// Program.cs
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));
builder.Services.AddOptions<EmailOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 5. Testing

Mock email service cho testing:

```csharp
public class FakeEmailService : IEmailService
{
    public Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[FAKE EMAIL] Verification sent to {email} with token {token}");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[FAKE EMAIL] Password reset sent to {email} with token {token}");
        return Task.CompletedTask;
    }
}

// Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}
```

---

## So sánh các Options

| Feature | SMTP | SendGrid | AWS SES | Azure CS |
|---------|------|----------|---------|----------|
| **Cost** | Free (Gmail) | Free 100/day | $0.10/1000 | Pay as you go |
| **Setup** | ⭐⭐⭐⭐⭐ Easy | ⭐⭐⭐⭐ Easy | ⭐⭐⭐ Medium | ⭐⭐⭐ Medium |
| **Deliverability** | ⭐⭐⭐ Good | ⭐⭐⭐⭐⭐ Excellent | ⭐⭐⭐⭐⭐ Excellent | ⭐⭐⭐⭐ Very Good |
| **Analytics** | ❌ No | ✅ Yes | ✅ Yes | ✅ Yes |
| **Templates** | ❌ Manual | ✅ Yes | ✅ Yes | ✅ Yes |
| **Scale** | ⭐⭐ Limited | ⭐⭐⭐⭐⭐ Unlimited | ⭐⭐⭐⭐⭐ Unlimited | ⭐⭐⭐⭐⭐ Unlimited |

## Khuyến nghị

- **Development/Testing**: SMTP với Gmail (dễ setup)
- **Small Projects**: SendGrid free tier
- **Production/Scale**: AWS SES hoặc SendGrid paid
- **Azure Ecosystem**: Azure Communication Services

---

## Troubleshooting

### Gmail SMTP không hoạt động
- Bật 2-Step Verification
- Tạo App Password (không dùng password thường)
- Kiểm tra "Less secure app access" (nếu không dùng 2FA)

### SendGrid emails vào Spam
- Verify domain
- Setup SPF, DKIM, DMARC records
- Warm up IP address

### AWS SES Sandbox Mode
- Chỉ gửi được đến verified emails
- Request production access để gửi đến bất kỳ email nào

### Rate Limiting
- Implement exponential backoff
- Sử dụng queue system
- Monitor sending rate

---

## Next Steps

1. Chọn provider phù hợp với project
2. Follow guide tương ứng
3. Test với development environment
4. Setup monitoring và logging
5. Deploy to production
