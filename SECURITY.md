# Security Policy

## Sensitive Information

This project uses configuration files that may contain sensitive information. Please follow these guidelines:

### ⚠️ DO NOT Commit

Never commit these files to version control:
- `appsettings.Development.json`
- `appsettings.Production.json`
- `*.env` files
- Database files (*.db, *.sqlite)
- Certificate files (*.pfx, *.key, *.pem)

### ✅ Safe to Commit

These files are safe to commit (they contain only templates/examples):
- `appsettings.json` (template with placeholders)
- `appsettings.Example.json` (example configuration)

## Configuration Setup

### For Development

1. Copy the example configuration:
   ```bash
   cp src/Services/Users/ECommerce.User.API/appsettings.Example.json \
      src/Services/Users/ECommerce.User.API/appsettings.Development.json
   ```

2. Update with your actual values:
   - Database credentials
   - JWT secret (generate a secure random string)
   - Email credentials (Gmail App Password)

3. Verify it's gitignored:
   ```bash
   git status
   # Should NOT show appsettings.Development.json
   ```

### For Production

Use environment variables or secret management services:
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Kubernetes Secrets

## Secrets Management

### JWT Secret
- Minimum 32 characters
- Use cryptographically secure random generation
- Different secret for each environment
- Rotate regularly

### Database Passwords
- Use strong passwords (16+ characters)
- Different password for each environment
- Use managed database services with IAM when possible

### Email Credentials
- Use App Passwords, not account passwords
- Limit permissions to "Send Mail" only
- Consider using dedicated email service (SendGrid, AWS SES)

## Reporting Security Issues

If you discover a security vulnerability, please email: [security@yourdomain.com]

**DO NOT** create a public GitHub issue for security vulnerabilities.

## Security Checklist

Before committing code:

- [ ] No passwords in code or config files
- [ ] No API keys or secrets in code
- [ ] `appsettings.Development.json` is gitignored
- [ ] `appsettings.json` contains only placeholders
- [ ] Sensitive data uses environment variables in production
- [ ] JWT secrets are strong and unique per environment

## Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

---

**Last Updated:** November 2, 2025
