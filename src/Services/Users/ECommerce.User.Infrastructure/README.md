# ECommerce.User.Infrastructure

Infrastructure layer cho User Service - chứa database access, repositories, và external services.

## Structure

```
ECommerce.User.Infrastructure/
├── Data/
│   ├── UserDbContext.cs
│   └── Configurations/
│       ├── UserConfiguration.cs
│       ├── RoleConfiguration.cs
│       ├── UserRoleConfiguration.cs
│       ├── AddressConfiguration.cs
│       ├── UserSessionConfiguration.cs
│       └── AuditLogConfiguration.cs
└── Repositories/
    └── UserRepository.cs
```

## Database: PostgreSQL

### Connection String
```json
{
  "ConnectionStrings": {
    "UserDb": "Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=password"
  }
}
```

## DbContext

**UserDbContext** - Entity Framework Core DbContext

**DbSets:**
- Users
- Roles
- UserRoles
- Addresses
- UserSessions
- AuditLogs

## Entity Configurations

All entities are configured using Fluent API with `IEntityTypeConfiguration<T>`.

**Features:**
- Snake_case column names (PostgreSQL convention)
- Proper indexes for performance
- Soft delete support (DeletedAt)
- Default values
- Relationships (One-to-Many, Many-to-Many)
- Seed data for Roles

## Repositories

### IUserRepository

```csharp
Task<User?> GetByIdAsync(Guid id);
Task<User?> GetByEmailAsync(string email);
Task<User?> GetByUsernameAsync(string username);
Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);
Task<User> CreateAsync(User user);
Task<User> UpdateAsync(User user);
Task<bool> DeleteAsync(Guid id); // Soft delete
Task<bool> ExistsAsync(Guid id);
Task<bool> EmailExistsAsync(string email);
Task<int> GetTotalCountAsync();
```

## Migrations

### Create Migration
```bash
dotnet ef migrations add InitialCreate --project src/Services/Users/ECommerce.User.Infrastructure --startup-project src/Services/Users/ECommerce.User.API
```

### Apply Migration
```bash
dotnet ef database update --project src/Services/Users/ECommerce.User.Infrastructure --startup-project src/Services/Users/ECommerce.User.API
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project src/Services/Users/ECommerce.User.Infrastructure --startup-project src/Services/Users/ECommerce.User.API
```

## Dependencies

- Microsoft.EntityFrameworkCore (9.0.10)
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.4)
- Microsoft.EntityFrameworkCore.Design (9.0.10)

## Usage Example

```csharp
// In Program.cs
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddScoped<IUserRepository, UserRepository>();

// In Service
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }
}
```

---

**Status**: ✅ Complete  
**Build Status**: ✅ Builds successfully  
**Next Step**: Create migrations and update database
