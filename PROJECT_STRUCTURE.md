# Project Structure

## Important Files

### Root Level
```
.coderabbit.yaml              # CodeRabbit AI configuration
.gitignore                    # Git ignore patterns
DEVELOPMENT_NOTES.md          # Quick reference for features
SECURITY.md                   # Security policy
```

### Configuration (Public)
```
src/Services/Users/ECommerce.User.API/
├── appsettings.json          # Template (safe to commit)
├── appsettings.Example.json  # Example config
└── CONFIGURATION.md          # Setup instructions
```

### Configuration (Private - Gitignored)
```
src/Services/Users/ECommerce.User.API/
├── appsettings.Development.json  # Your local config
└── test-email.http               # Test requests
```

### Documentation (Public)
```
docs/
├── email-verification-password-reset-guide.md
├── email-service-implementation-guide.md
└── user-service-implementation-guide.md
```

### Documentation (Private - Gitignored)
```
docs/guides/
├── README.md
├── START_HERE.md
├── READY_TO_TEST.md
├── TEST_INSTRUCTIONS.md
└── test-setup.ps1
```

### GitHub
```
.github/
├── workflows/
│   └── coderabbit.yml        # CodeRabbit workflow
└── pull_request_template.md  # PR template
```

## Quick Reference

### Setup
```bash
# Copy example config
cp src/Services/Users/ECommerce.User.API/appsettings.Example.json \
   src/Services/Users/ECommerce.User.API/appsettings.Development.json

# Edit with your credentials
# Run
cd src/Services/Users/ECommerce.User.API
dotnet run
```

### Before Commit
```bash
# Verify no sensitive files
git status

# Should NOT see:
# - appsettings.Development.json
# - docs/guides/
# - *.http files
```

### CodeRabbit
- Install: https://github.com/apps/coderabbitai
- Config: `.coderabbit.yaml`
- Usage: `@coderabbitai help` in PR comments

---

**Keep it simple!** Only essential files documented here.
