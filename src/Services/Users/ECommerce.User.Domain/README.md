# ECommerce.User.Domain

Domain layer cho User Service - chứa business entities và domain logic.

## Entities

### User
Core entity đại diện cho người dùng trong hệ thống.

**Properties:**
- `Id` - Unique identifier (Guid)
- `Email` - Email address (unique, required)
- `Username` - Username (unique, optional)
- `PasswordHash` - Hashed password
- `FirstName`, `LastName` - User's name
- `PhoneNumber` - Contact number
- `DateOfBirth` - Date of birth
- `EmailVerified` - Email verification status
- `IsActive`, `IsLocked` - Account status
- `FailedLoginAttempts` - Failed login counter for brute-force protection
- `LockoutEnd` - When account lockout expires (auto-unlock)
- `LastLoginAt` - Last successful login timestamp

**Navigation Properties:**
- `UserRoles` - User's assigned roles
- `Addresses` - User's addresses
- `Sessions` - Active sessions

**Computed Properties:**
- `FullName` - Combined first and last name
- `IsEmailVerificationTokenValid` - Check if email token is valid
- `IsPasswordResetTokenValid` - Check if reset token is valid
- `IsLockedOut` - Check if account is currently locked out
- `CanLogin` - Check if user can login (active, not locked, email verified)

**Helper Methods:**
- `IncrementFailedLoginAttempts(maxAttempts, lockoutMinutes)` - Increment failed attempts and lock if needed
- `ResetFailedLoginAttempts()` - Reset counter on successful login
- `UnlockAccount()` - Manually unlock account

---

### Role
Represents user roles in the system.

**Properties:**
- `Id` - Role identifier
- `Name` - Role name (Admin, Manager, Customer, Guest)
- `Description` - Role description

**Navigation Properties:**
- `UserRoles` - Users with this role

---

### UserRole
Many-to-many relationship between User and Role.

**Properties:**
- `UserId` - User identifier
- `RoleId` - Role identifier
- `AssignedAt` - When role was assigned
- `AssignedBy` - Who assigned the role

---

### Address
User's shipping/billing addresses.

**Properties:**
- `Id` - Address identifier
- `UserId` - Owner user
- `AddressType` - Shipping or Billing
- `StreetAddress`, `Apartment` - Street info
- `City`, `StateProvince`, `PostalCode`, `Country` - Location
- `IsDefault` - Default address flag

**Computed Properties:**
- `FullAddress` - Formatted full address

---

### UserSession
Tracks user sessions and refresh tokens.

**Properties:**
- `Id` - Session identifier
- `UserId` - User identifier
- `RefreshToken` - JWT refresh token
- `DeviceInfo`, `IpAddress`, `UserAgent` - Session metadata
- `IsActive` - Session status
- `ExpiresAt` - Expiration time

**Computed Properties:**
- `IsExpired` - Check if session expired
- `IsValid` - Check if session is valid and active

---

### AuditLog
Audit trail for user actions.

**Properties:**
- `Id` - Log identifier
- `UserId` - User who performed action
- `Action` - Action performed
- `EntityType`, `EntityId` - Affected entity
- `OldValues`, `NewValues` - Change tracking (JSON)
- `IpAddress`, `UserAgent` - Request metadata
- `Timestamp` - When action occurred

---

## Enums

### RoleType
```csharp
public enum RoleType
{
    Guest = 1,
    Customer = 2,
    Manager = 3,
    Admin = 4
}
```

### AddressType
```csharp
public enum AddressType
{
    Shipping,
    Billing
}
```

---

## Design Principles

1. **No External Dependencies**: Domain layer has no dependencies on other projects
2. **Rich Domain Model**: Entities contain business logic
3. **Computed Properties**: Derived values calculated in entities
4. **Navigation Properties**: EF Core relationships
5. **Immutability**: Use init-only setters where appropriate
6. **Validation**: Basic validation in entities, complex validation in Application layer

---

## Usage Example

```csharp
// Create new user
var user = new User
{
    Email = "user@example.com",
    FirstName = "John",
    LastName = "Doe",
    PasswordHash = hashedPassword,
    EmailVerified = false
};

// Add address
var address = new Address
{
    UserId = user.Id,
    AddressType = "shipping",
    StreetAddress = "123 Main St",
    City = "New York",
    PostalCode = "10001",
    Country = "USA",
    IsDefault = true
};

user.Addresses.Add(address);

// Check computed properties
Console.WriteLine(user.FullName); // "John Doe"
Console.WriteLine(address.FullAddress); // "123 Main St, New York, 10001, USA"

// Account lockout example
if (!passwordValid)
{
    user.IncrementFailedLoginAttempts(maxAttempts: 5, lockoutMinutes: 15);
    // After 5 failed attempts: IsLocked = true, LockoutEnd = now + 15 minutes
}
else
{
    user.ResetFailedLoginAttempts();
    // FailedLoginAttempts = 0, IsLocked = false, LastLoginAt = now
}

// Check if user can login
if (user.CanLogin)
{
    // Proceed with login
}
else if (user.IsLockedOut)
{
    var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
    Console.WriteLine($"Account locked. Try again in {remainingTime.Minutes} minutes");
}
```

---

**Status**: ✅ Complete  
**Build Status**: ✅ Builds successfully  
**Dependencies**: None (Pure .NET 9.0)
