using System.Net;
using System.Net.Mail;
using ECommerce.Notification.Application.Interfaces;
using ECommerce.Notification.Domain.Entities;
using ECommerce.Notification.Domain.Enums;
using ECommerce.Notification.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly INotificationLogRepository _notificationLogRepository;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IEmailLogRepository emailLogRepository,
        INotificationLogRepository notificationLogRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _emailLogRepository = emailLogRepository;
        _notificationLogRepository = notificationLogRepository;
    }

    public async Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount, string customerName)
    {
        var subject = $"Xác nhận đơn hàng #{orderNumber}";
        var body = GetOrderConfirmationTemplate(orderNumber, totalAmount, customerName);
        await SendEmailAsync(email, subject, body, "order-confirmation", new Dictionary<string, string>
        {
            { "orderNumber", orderNumber },
            { "totalAmount", totalAmount.ToString() },
            { "customerName", customerName }
        });
    }

    public async Task SendPaymentReceiptAsync(string email, string orderNumber, string paymentNumber, decimal amount)
    {
        var subject = $"Biên lai thanh toán - Đơn hàng #{orderNumber}";
        var body = GetPaymentReceiptTemplate(orderNumber, paymentNumber, amount);
        await SendEmailAsync(email, subject, body, "payment-receipt", new Dictionary<string, string>
        {
            { "orderNumber", orderNumber },
            { "paymentNumber", paymentNumber },
            { "amount", amount.ToString() }
        });
    }

    public async Task SendOrderShippedAsync(string email, string orderNumber, string trackingNumber, string customerName)
    {
        var subject = $"Đơn hàng #{orderNumber} đã được giao cho đơn vị vận chuyển";
        var body = GetOrderShippedTemplate(orderNumber, trackingNumber, customerName);
        await SendEmailAsync(email, subject, body, "order-shipped", new Dictionary<string, string>
        {
            { "orderNumber", orderNumber },
            { "trackingNumber", trackingNumber },
            { "customerName", customerName }
        });
    }

    public async Task SendOrderDeliveredAsync(string email, string orderNumber, string customerName)
    {
        var subject = $"Đơn hàng #{orderNumber} đã được giao thành công";
        var body = GetOrderDeliveredTemplate(orderNumber, customerName);
        await SendEmailAsync(email, subject, body, "order-delivered", new Dictionary<string, string>
        {
            { "orderNumber", orderNumber },
            { "customerName", customerName }
        });
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Chào mừng bạn đến với ECommerce!";
        var body = GetWelcomeEmailTemplate(firstName);
        await SendEmailAsync(email, subject, body, "welcome-email", new Dictionary<string, string>
        {
            { "firstName", firstName }
        });
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, string emailType, Dictionary<string, string>? metadata = null)
    {
        var fromEmail = _configuration["Email:Smtp:FromEmail"] ?? "noreply@ecommerce.com";
        
        // Create notification log
        var notificationLog = new NotificationLog
        {
            UserId = "system", // Will be updated by event handlers with actual userId
            Type = NotificationType.Email,
            Channel = NotificationChannel.Order,
            Subject = subject,
            Message = body,
            Recipient = new RecipientInfo { Email = toEmail },
            Status = NotificationStatus.Queued,
            Metadata = metadata ?? new Dictionary<string, string>()
        };
        
        await _notificationLogRepository.CreateAsync(notificationLog);

        // Create email log
        var emailLog = new EmailLog
        {
            NotificationId = notificationLog.Id,
            UserId = "system",
            From = fromEmail,
            To = toEmail,
            Subject = subject,
            BodyHtml = body,
            BodyText = body,
            Status = NotificationStatus.Queued
        };
        
        await _emailLogRepository.CreateAsync(emailLog);

        try
        {
            var smtpHost = _configuration["Email:Smtp:Host"] 
                ?? throw new InvalidOperationException("SMTP Host not configured");
            var smtpPort = int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Email:Smtp:EnableSsl"] ?? "true");
            var username = _configuration["Email:Smtp:Username"] 
                ?? throw new InvalidOperationException("SMTP Username not configured");
            var password = _configuration["Email:Smtp:Password"] 
                ?? throw new InvalidOperationException("SMTP Password not configured");
            var fromName = _configuration["Email:Smtp:FromName"] ?? "ECommerce Platform";

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

            await client.SendMailAsync(message);

            // Update logs as sent
            notificationLog.Status = NotificationStatus.Sent;
            notificationLog.SentAt = DateTime.UtcNow;
            await _notificationLogRepository.UpdateAsync(notificationLog);

            emailLog.Status = NotificationStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            await _emailLogRepository.UpdateAsync(emailLog);

            _logger.LogInformation("Email sent successfully to {Email} - Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            // Update logs as failed
            notificationLog.Status = NotificationStatus.Failed;
            notificationLog.FailedAt = DateTime.UtcNow;
            notificationLog.Error = new ErrorInfo
            {
                Code = "SMTP_ERROR",
                Message = ex.Message,
                Details = ex.StackTrace
            };
            await _notificationLogRepository.UpdateAsync(notificationLog);

            emailLog.Status = NotificationStatus.Failed;
            emailLog.Error = new ErrorInfo
            {
                Code = "SMTP_ERROR",
                Message = ex.Message
            };
            await _emailLogRepository.UpdateAsync(emailLog);

            _logger.LogError(ex, "Failed to send email to {Email} - Subject: {Subject}", toEmail, subject);
            // Don't throw - notification failures shouldn't break the system
        }
    }

    private string GetOrderConfirmationTemplate(string orderNumber, decimal totalAmount, string customerName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: white; padding: 30px; margin-top: 20px; }}
        .order-info {{ background-color: #f0f0f0; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .total {{ font-size: 24px; font-weight: bold; color: #007bff; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Đơn hàng đã được xác nhận!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Cảm ơn bạn đã đặt hàng tại ECommerce! Đơn hàng của bạn đã được xác nhận và đang được xử lý.</p>
            
            <div class='order-info'>
                <p><strong>Mã đơn hàng:</strong> {orderNumber}</p>
                <p><strong>Tổng tiền:</strong> <span class='total'>{totalAmount:N0} ₫</span></p>
            </div>

            <p>Chúng tôi sẽ thông báo cho bạn khi đơn hàng được giao cho đơn vị vận chuyển.</p>
            
            <p>Bạn có thể theo dõi đơn hàng của mình tại: <a href='http://localhost:3000/orders/{orderNumber}'>Xem đơn hàng</a></p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2024 ECommerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPaymentReceiptTemplate(string orderNumber, string paymentNumber, decimal amount)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: white; padding: 30px; margin-top: 20px; }}
        .receipt {{ background-color: #f0f0f0; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .amount {{ font-size: 28px; font-weight: bold; color: #28a745; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💳 Thanh toán thành công!</h1>
        </div>
        <div class='content'>
            <p>Cảm ơn bạn đã thanh toán!</p>
            <p>Chúng tôi đã nhận được thanh toán của bạn và đơn hàng đang được xử lý.</p>
            
            <div class='receipt'>
                <p><strong>Mã thanh toán:</strong> {paymentNumber}</p>
                <p><strong>Mã đơn hàng:</strong> {orderNumber}</p>
                <p><strong>Số tiền:</strong> <span class='amount'>{amount:N0} ₫</span></p>
                <p><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>

            <p>Biên lai chi tiết đã được đính kèm trong email này.</p>
            <p>Đơn hàng của bạn sẽ được giao trong 2-3 ngày làm việc.</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2024 ECommerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrderShippedTemplate(string orderNumber, string trackingNumber, string customerName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: white; padding: 30px; margin-top: 20px; }}
        .tracking {{ background-color: #e7f3ff; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #17a2b8; }}
        .tracking-number {{ font-size: 20px; font-weight: bold; color: #17a2b8; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚚 Đơn hàng đang được giao!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Đơn hàng <strong>{orderNumber}</strong> của bạn đã được giao cho đơn vị vận chuyển!</p>
            
            <div class='tracking'>
                <p><strong>Mã vận đơn:</strong></p>
                <p class='tracking-number'>{trackingNumber}</p>
                <p style='margin-top: 15px;'>Bạn có thể theo dõi đơn hàng tại đây:</p>
                <a href='https://tracking.example.com/{trackingNumber}' style='display: inline-block; padding: 10px 20px; background-color: #17a2b8; color: white; text-decoration: none; border-radius: 5px; margin-top: 10px;'>Theo dõi đơn hàng</a>
            </div>

            <p>Đơn hàng dự kiến sẽ được giao trong 2-3 ngày làm việc.</p>
            <p>Vui lòng kiểm tra hàng trước khi thanh toán (nếu COD).</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2024 ECommerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrderDeliveredTemplate(string orderNumber, string customerName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: white; padding: 30px; margin-top: 20px; }}
        .success {{ background-color: #d4edda; padding: 20px; margin: 20px 0; border-radius: 5px; border-left: 4px solid #28a745; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đơn hàng đã được giao!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            
            <div class='success'>
                <p style='font-size: 18px; margin: 0;'>✅ Đơn hàng <strong>{orderNumber}</strong> đã được giao thành công!</p>
            </div>

            <p>Cảm ơn bạn đã mua sắm tại ECommerce!</p>
            <p>Chúng tôi hy vọng bạn hài lòng với sản phẩm.</p>
            
            <p style='margin-top: 30px;'>Nếu bạn có bất kỳ vấn đề gì với đơn hàng, vui lòng liên hệ với chúng tôi trong vòng 7 ngày để được hỗ trợ đổi trả.</p>
            
            <p>Đánh giá sản phẩm giúp chúng tôi cải thiện dịch vụ:</p>
            <a href='http://localhost:3000/orders/{orderNumber}/review' style='display: inline-block; padding: 10px 20px; background-color: #ffc107; color: #333; text-decoration: none; border-radius: 5px; margin-top: 10px;'>⭐ Đánh giá đơn hàng</a>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2024 ECommerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #6f42c1; color: white; padding: 30px; text-align: center; }}
        .content {{ background-color: white; padding: 30px; margin-top: 20px; }}
        .features {{ margin: 30px 0; }}
        .feature {{ padding: 15px; margin: 10px 0; background-color: #f8f9fa; border-radius: 5px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎊 Chào mừng đến với ECommerce!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{firstName}</strong>,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại ECommerce!</p>
            
            <p>Chúng tôi rất vui được chào đón bạn. Dưới đây là một số điều bạn có thể làm:</p>
            
            <div class='features'>
                <div class='feature'>
                    <strong>🛍️ Mua sắm hàng ngàn sản phẩm</strong>
                    <p>Khám phá bộ sưu tập đa dạng với giá tốt nhất</p>
                </div>
                <div class='feature'>
                    <strong>🚚 Giao hàng nhanh chóng</strong>
                    <p>Miễn phí vận chuyển cho đơn hàng trên 500.000₫</p>
                </div>
                <div class='feature'>
                    <strong>💳 Thanh toán an toàn</strong>
                    <p>Nhiều phương thức thanh toán tiện lợi</p>
                </div>
                <div class='feature'>
                    <strong>🎁 Ưu đãi độc quyền</strong>
                    <p>Nhận thông báo về các chương trình khuyến mãi</p>
                </div>
            </div>

            <p style='text-align: center; margin-top: 30px;'>
                <a href='http://localhost:3000' style='display: inline-block; padding: 15px 30px; background-color: #6f42c1; color: white; text-decoration: none; border-radius: 5px; font-size: 16px;'>Bắt đầu mua sắm</a>
            </p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>© 2024 ECommerce Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
