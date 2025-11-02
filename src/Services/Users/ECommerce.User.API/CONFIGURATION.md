# Configuration Guide

## Setup Instructions

### 1. Copy Example Configuration

```bash
cp appsettings.Example.json appsettings.Development.json
```

### 2. Update Configuration Values

Edit `appsettings.Development.json` with your actual values:

#### Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=ECommerceUserDb;Username=postgres;Password=YOUR_PASSWORD"
}
```

#### JWT Secret
Generate a secure random string (at least 32 characters):
```bash
# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})

# Or use online generator: https://randomkeygen.com/
```

```json
"Jwt": {
  "Secret": "YOUR_GENERATED_SECRET_HERE"
}
```

#### Email Configuration (Gmail)

1. Enable 2-Step Verification in your Google Account
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Update configuration:

```json
"Email": {
  "Smtp": {
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "FromEmail": "your-email@gmail.com"
  }
}
```

## Environment Variables (Production)

For production, use environment variables instead of appsettings files:

```bash
# Database
ConnectionStrings__DefaultConnection="Host=prod-server;Database=ECommerceUserDb;Username=prod_user;Password=secure_password"

# JWT
Jwt__Secret="your_production_secret_key"

# Email
Email__Smtp__Username="production@yourdomain.com"
Email__Smtp__Password="production_app_password"
```

## Security Best Practices

1. ✅ Never commit `appsettings.Development.json` to git
2. ✅ Use different secrets for each environment
3. ✅ Use Azure Key Vault or AWS Secrets Manager in production
4. ✅ Rotate secrets regularly
5. ✅ Use strong, randomly generated JWT secrets (min 32 chars)

## Files

- `appsettings.json` - Template with placeholders (safe to commit)
- `appsettings.Example.json` - Example configuration (safe to commit)
- `appsettings.Development.json` - Your local config (gitignored, DO NOT commit)
- `appsettings.Production.json` - Production config (gitignored, DO NOT commit)

## Verification

Check that sensitive files are gitignored:

```bash
git status
# Should NOT show:
# - appsettings.Development.json
# - appsettings.Production.json
```

## Need Help?

See `docs/guides/` for detailed setup instructions.
