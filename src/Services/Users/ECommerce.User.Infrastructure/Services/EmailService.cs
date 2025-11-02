using ECommerce.User.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

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
        // TODO: Implement actual email sending logic (SMTP, SendGrid, etc.)
        var verificationUrl = $"{_configuration["App:BaseUrl"]}/api/auth/verify-email?token={token}";

        var subject = "Verify Your Email Address";
        var body = GetEmailVerificationTemplate(verificationUrl);

        // Simulate async operation
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending logic (SMTP, SendGrid, etc.)
        var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&email={email}";

        var subject = "Reset Your Password";
        var body = GetPasswordResetTemplate(resetUrl);

        // Simulate async operation
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
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
            var fromEmail = _configuration["Email:Smtp:FromEmail"] 
                ?? throw new InvalidOperationException("SMTP FromEmail not configured");
            var fromName = _configuration["Email:Smtp:FromName"] ?? "ECommerce Platform";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(username, password),
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