# User Service - Clean Architecture

## Project Structure

```
ECommerce.User.API/
├── Controllers/
├── Middleware/
├── Program.cs
└── References:
    ├── ECommerce.User.Application
    └── ECommerce.User.Infrastructure

ECommerce.User.Application/
├── DTOs/
├── Services/
├── Interfaces/
├── Validators/
└── References:
    └── ECommerce.User.Domain

ECommerce.User.Domain/
├── Entities/
├── ValueObjects/
├── Enums/
└── Interfaces/
    (No external references - Pure domain logic)

ECommerce.User.Infrastructure/
├── Data/
├── Repositories/
├── Services/
└── References:
    ├── ECommerce.User.Domain
    └── ECommerce.User.Application
```

## Dependency Flow

```
┌─────────────────────────────────────────┐
│     ECommerce.User.API (Presentation)   │
│  - Controllers                          │
│  - Middleware                           │
│  - Program.cs                           │
└────────────┬────────────────────────────┘
             │ references
             ↓
┌────────────┴────────────────────────────┐
│  ECommerce.User.Application (Business)  │
│  - DTOs                                 │
│  - Services                             │
│  - Validators                           │
└────────────┬────────────────────────────┘
             │ references
             ↓
┌────────────┴────────────────────────────┐
│    ECommerce.User.Domain (Core)         │
│  - Entities                             │
│  - Value Objects                        │
│  - Domain Interfaces                    │
└─────────────────────────────────────────┘
             ↑
             │ references
┌────────────┴────────────────────────────┐
│  ECommerce.User.Infrastructure (Data)   │
│  - DbContext                            │
│  - Repositories                         │
│  - External Services                    │
└─────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. API Layer (Presentation)
**Purpose**: Handle HTTP requests and responses

**Responsibilities**:
- Controllers for REST endpoints
- Request/Response models
- Authentication/Authorization
- Middleware (Exception handling, logging)
- Dependency injection configuration

**Dependencies**: Application + Infrastructure

---

### 2. Application Layer (Business Logic)
**Purpose**: Orchestrate business workflows

**Responsibilities**:
- Application services
- DTOs (Data Transfer Objects)
- Input validation (FluentValidation)
- Business logic coordination
- Interface definitions for infrastructure

**Dependencies**: Domain only

---

### 3. Domain Layer (Core)
**Purpose**: Business entities and rules

**Responsibilities**:
- Domain entities
- Value objects
- Domain events
- Business rules
- Repository interfaces
- **NO external dependencies**

**Dependencies**: None (pure .NET)

---

### 4. Infrastructure Layer (Data Access)
**Purpose**: External concerns implementation

**Responsibilities**:
- Database context (EF Core)
- Repository implementations
- External service integrations
- Caching
- Message bus

**Dependencies**: Domain + Application

---

## Dependency Rules

### ✅ Allowed Dependencies

```
API → Application
API → Infrastructure
Application → Domain
Infrastructure → Domain
Infrastructure → Application
```

### ❌ Forbidden Dependencies

```
Domain → Application  ❌
Domain → Infrastructure  ❌
Domain → API  ❌
Application → Infrastructure  ❌
Application → API  ❌
```

---

## Benefits of This Architecture

1. **Separation of Concerns**: Each layer has clear responsibility
2. **Testability**: Easy to unit test business logic
3. **Maintainability**: Changes in one layer don't affect others
4. **Flexibility**: Easy to swap implementations
5. **Domain-Centric**: Business logic is independent

---

## Example Flow

### User Registration Flow

```
1. HTTP POST /api/users/register
   ↓
2. UsersController (API Layer)
   ↓
3. IUserService.RegisterAsync() (Application Layer)
   ↓
4. User entity validation (Domain Layer)
   ↓
5. IUserRepository.CreateAsync() (Infrastructure Layer)
   ↓
6. PostgreSQL Database
```

---

## Project References Summary

| Project | References |
|---------|-----------|
| **ECommerce.User.API** | Application, Infrastructure |
| **ECommerce.User.Application** | Domain |
| **ECommerce.User.Domain** | None |
| **ECommerce.User.Infrastructure** | Domain, Application |

---

**Status**: ✅ Project references configured correctly  
**Build Status**: ✅ All projects build successfully  
**Architecture**: Clean Architecture (Onion Architecture)
