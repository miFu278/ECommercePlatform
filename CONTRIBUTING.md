# Contributing to E-Commerce Microservices Platform

Thank you for your interest in contributing! 🎉

## How to Contribute

### 1. Fork the Repository

Click the "Fork" button at the top right of the repository page.

### 2. Clone Your Fork

```bash
git clone https://github.com/yourusername/ECommercePlatform.git
cd ECommercePlatform
```

### 3. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring

### 4. Make Your Changes

- Follow the existing code style
- Write clean, readable code
- Add comments where necessary
- Update documentation if needed

### 5. Test Your Changes

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Test locally with Docker
cd docker
docker-compose -f docker-compose.infrastructure.yml up -d
```

### 6. Commit Your Changes

```bash
git add .
git commit -m "feat: add amazing feature"
```

Commit message format:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

### 7. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 8. Create a Pull Request

1. Go to the original repository
2. Click "New Pull Request"
3. Select your fork and branch
4. Fill in the PR template
5. Submit!

## Code Style Guidelines

### C# Coding Standards

- Use PascalCase for classes, methods, properties
- Use camelCase for local variables, parameters
- Prefix interfaces with 'I' (IUserRepository)
- Use async/await for asynchronous operations
- Add XML comments for public APIs

### Project Structure

Follow Clean Architecture:
```
Service/
├── API/              # Controllers, Middleware
├── Application/      # DTOs, Services, Validators
├── Domain/           # Entities, Interfaces
└── Infrastructure/   # Data access, External services
```

### Testing

- Write unit tests for business logic
- Write integration tests for APIs
- Aim for >70% code coverage

## Pull Request Guidelines

### PR Title Format

```
[Type] Brief description

Examples:
[Feature] Add user authentication
[Bugfix] Fix login validation
[Docs] Update API documentation
```

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests pass locally
```

## Development Setup

### Prerequisites

- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code
- Git

### Local Development

1. Start infrastructure:
```bash
cd docker
.\start.ps1
```

2. Run migrations:
```bash
dotnet ef database update
```

3. Run service:
```bash
dotnet run
```

## Questions?

- Open an issue for bugs or feature requests
- Start a discussion for questions
- Contact maintainers via email

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Follow the project's code of conduct

Thank you for contributing! 🚀
