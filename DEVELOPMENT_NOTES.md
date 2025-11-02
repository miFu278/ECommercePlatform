# Development Notes

## Completed Features ✅

### Email Verification & Password Reset
- Email verification (24h token)
- Forgot password flow
- Password reset (1h token)
- SMTP Gmail integration
- HTML email templates

### Tech Stack
- ASP.NET Core 9.0
- PostgreSQL
- JWT Authentication
- FluentValidation
- AutoMapper

### API Endpoints
```
POST   /api/auth/register
GET    /api/auth/verify-email
POST   /api/auth/login
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
POST   /api/auth/refresh-token
POST   /api/auth/logout
```

### Configuration
- Port: 5000
- Database: PostgreSQL
- Email: Gmail SMTP

## Important Files

### Configuration
- `appsettings.json` - Template (safe to commit)
- `appsettings.Development.json` - Local config (gitignored)
- `appsettings.Example.json` - Example config

### Documentation
- `SECURITY.md` - Security policy
- `src/Services/Users/ECommerce.User.API/CONFIGURATION.md` - Setup guide
- `docs/guides/` - Private development guides (gitignored)

### Testing
- `src/Services/Users/ECommerce.User.API/test-email.http` - Test requests (gitignored)

## Quick Start

```bash
# Setup
cd src/Services/Users/ECommerce.User.API
cp appsettings.Example.json appsettings.Development.json
# Edit appsettings.Development.json with your credentials

# Run
dotnet run

# Test
# Open test-email.http and send requests
```

## CodeRabbit AI Review

- Configuration: `.coderabbit.yaml`
- PR Template: `.github/pull_request_template.md`
- Workflow: `.github/workflows/coderabbit.yml`

Install: https://github.com/apps/coderabbitai

---

**Last Updated:** November 2, 2025
