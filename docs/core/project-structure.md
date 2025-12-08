# Cấu Trúc Dự Án E-Commerce Platform

## Tổng Quan

Dự án được tổ chức theo kiến trúc microservices với Clean Architecture, mỗi service độc lập và có cấu trúc riêng.

## Cấu Trúc Thư Mục Chính

```
ECommercePlatform/
├── src/                          # Source code
│   ├── ApiGateway/              # API Gateway (Ocelot)
│   ├── Services/                # Microservices
│   └── BuildingBlocks/          # Shared libraries
├── docker/                       # Docker configurations
├── docs/                         # Documentation
├── ECommercePlatform.sln        # Solution file
├── README.md                     # Tài liệu chính
├── COMPLETENESS_REPORT.md       # Báo cáo tiến độ
├── CONTRIBUTING.md              # Hướng dẫn đóng góp
└── LICENSE                       # Giấy phép

```

## Chi Tiết Cấu Trúc

### 1. API Gateway

```
src/ApiGateway/
└── ECommerce.ApiGateway/
    ├── Program.cs                # Entry point
    ├── appsettings.json         # Configuration
    ├── ocelot.json              # Ocelot routing config
    └── Dockerfile               # Docker image
```

**Chức năng:**
- Định tuyến requests đến các microservices
- JWT authentication
- Rate limiting
- Load balancing
- Request/Response transformation

### 2. Microservices

Mỗi microservice tuân theo Clean Architecture với 4 layers:

```
src/Services/{ServiceName}/
├── ECommerce.{ServiceName}.API/           # Presentation Layer
│   ├── Controllers/                       # API Controllers
│   ├── Middleware/                        # Custom middleware
│   ├── Grpc/                             # gRPC services
│   ├── Program.cs                        # Entry point
│   ├── appsettings.json                  # Configuration
│   └── Dockerfile                        # Docker image
│
├── ECommerce.{ServiceName}.Application/   # Application Layer
│   ├── DTOs/                             # Data Transfer Objects
│   ├── Services/                         # Application services
│   ├── Interfaces/                       # Service interfaces
│   ├── Validators/                       # FluentValidation
│   ├── Mappings/                         # AutoMapper profiles
│   └── EventHandlers/                    # Event handlers
│
├── ECommerce.{ServiceName}.Domain/        # Domain Layer
│   ├── Entities/                         # Domain entities
│   ├── ValueObjects/                     # Value objects
│   ├── Enums/                            # Enumerations
│   ├── Interfaces/                       # Repository interfaces
│   └── Events/                           # Domain events
│
└── ECommerce.{ServiceName}.Infrastructure/ # Infrastructure Layer
    ├── Data/                             # DbContext, configurations
    ├── Repositories/                     # Repository implementations
    ├── Services/                         # External service implementations
    ├── Migrations/                       # EF Core migrations
    └── MessageBus/                       # Event bus implementations
```

### 3. User Service

```
src/Services/Users/
├── ECommerce.User.API/
│   ├── Controllers/
│   │   ├── AuthController.cs            # Authentication endpoints
│   │   ├── UsersController.cs           # User management
│   │   └── AddressesController.cs       # Address management
│   ├── Grpc/
│   │   └── UserGrpcService.cs           # gRPC service
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs       # Global exception handling
│   └── Program.cs
│
├── ECommerce.User.Application/
│   ├── DTOs/
│   │   ├── Auth/                        # Login, Register DTOs
│   │   ├── User/                        # User DTOs
│   │   └── Address/                     # Address DTOs
│   ├── Services/
│   │   ├── AuthService.cs               # Authentication logic
│   │   ├── UserService.cs               # User management logic
│   │   └── TokenService.cs              # JWT token generation
│   ├── Validators/
│   │   ├── RegisterValidator.cs
│   │   └── LoginValidator.cs
│   └── Mappings/
│       └── UserMappingProfile.cs
│
├── ECommerce.User.Domain/
│   ├── Entities/
│   │   ├── User.cs                      # User entity
│   │   ├── Role.cs                      # Role entity
│   │   ├── Address.cs                   # Address entity
│   │   └── UserSession.cs               # Session entity
│   ├── Enums/
│   │   └── UserRole.cs
│   └── Interfaces/
│       ├── IUserRepository.cs
│       └── IAddressRepository.cs
│
└── ECommerce.User.Infrastructure/
    ├── Data/
    │   ├── UserDbContext.cs             # EF Core DbContext
    │   └── Configurations/              # Entity configurations
    ├── Repositories/
    │   ├── UserRepository.cs
    │   └── AddressRepository.cs
    ├── Services/
    │   └── PasswordHasher.cs            # Password hashing
    └── Migrations/                       # EF Core migrations
```

**Database:** PostgreSQL  
**Port:** 5001

### 4. Product Service

```
src/Services/Product/
├── ECommerce.Product.API/
│   ├── Controllers/
│   │   ├── ProductsController.cs        # Product CRUD
│   │   ├── CategoriesController.cs      # Category management
│   │   └── TagsController.cs            # Tag management
│   ├── Grpc/
│   │   └── ProductGrpcService.cs        # gRPC service
│   └── Program.cs
│
├── ECommerce.Product.Application/
│   ├── DTOs/
│   │   ├── Product/                     # Product DTOs
│   │   ├── Category/                    # Category DTOs
│   │   └── Tag/                         # Tag DTOs
│   ├── Services/
│   │   ├── ProductService.cs            # Product logic
│   │   ├── CategoryService.cs           # Category logic
│   │   └── ImageService.cs              # Image upload (Cloudinary)
│   └── Validators/
│       └── ProductValidator.cs
│
├── ECommerce.Product.Domain/
│   ├── Entities/
│   │   ├── Product.cs                   # Product entity
│   │   ├── Category.cs                  # Category entity
│   │   └── Tag.cs                       # Tag entity
│   ├── ValueObjects/
│   │   ├── Price.cs                     # Price value object
│   │   └── Image.cs                     # Image value object
│   └── Interfaces/
│       ├── IProductRepository.cs
│       └── ICategoryRepository.cs
│
└── ECommerce.Product.Infrastructure/
    ├── Data/
    │   ├── ProductDbContext.cs          # MongoDB context
    │   └── Configurations/              # MongoDB configurations
    ├── Repositories/
    │   ├── ProductRepository.cs
    │   └── CategoryRepository.cs
    └── Services/
        └── CloudinaryService.cs         # Image upload service
```

**Database:** MongoDB  
**Port:** 5002

### 5. Shopping Cart Service

```
src/Services/ShoppingCart/
├── ECommerce.ShoppingCart.API/
│   ├── Controllers/
│   │   └── CartController.cs            # Cart operations
│   └── Program.cs
│
├── ECommerce.ShoppingCart.Application/
│   ├── DTOs/
│   │   ├── Cart/                        # Cart DTOs
│   │   └── CartItem/                    # Cart item DTOs
│   ├── Services/
│   │   └── CartService.cs               # Cart logic
│   └── Validators/
│       └── AddToCartValidator.cs
│
├── ECommerce.ShoppingCart.Domain/
│   ├── Entities/
│   │   ├── Cart.cs                      # Cart entity
│   │   └── CartItem.cs                  # Cart item entity
│   └── Interfaces/
│       └── ICartRepository.cs
│
└── ECommerce.ShoppingCart.Infrastructure/
    ├── Data/
    │   └── RedisContext.cs              # Redis connection
    ├── Repositories/
    │   └── CartRepository.cs            # Redis repository
    └── Services/
        └── ProductGrpcClient.cs         # gRPC client to Product Service
```

**Database:** Redis  
**Port:** 5003

### 6. Order Service

```
src/Services/Order/
├── ECommerce.Order.API/
│   ├── Controllers/
│   │   └── OrdersController.cs          # Order operations
│   ├── Grpc/
│   │   └── OrderGrpcService.cs          # gRPC service
│   └── Program.cs
│
├── ECommerce.Order.Application/
│   ├── DTOs/
│   │   ├── Order/                       # Order DTOs
│   │   └── OrderItem/                   # Order item DTOs
│   ├── Services/
│   │   └── OrderService.cs              # Order logic
│   ├── EventHandlers/
│   │   └── PaymentCompletedHandler.cs   # Handle payment events
│   └── Validators/
│       └── CreateOrderValidator.cs
│
├── ECommerce.Order.Domain/
│   ├── Entities/
│   │   ├── Order.cs                     # Order entity
│   │   ├── OrderItem.cs                 # Order item entity
│   │   └── OrderStatusHistory.cs        # Status history
│   ├── Enums/
│   │   ├── OrderStatus.cs
│   │   └── PaymentStatus.cs
│   └── Interfaces/
│       └── IOrderRepository.cs
│
└── ECommerce.Order.Infrastructure/
    ├── Data/
    │   ├── OrderDbContext.cs            # EF Core DbContext
    │   └── Configurations/
    ├── Repositories/
    │   └── OrderRepository.cs
    ├── Services/
    │   ├── UserGrpcClient.cs            # gRPC client to User Service
    │   └── ProductGrpcClient.cs         # gRPC client to Product Service
    └── Migrations/
```

**Database:** PostgreSQL  
**Port:** 5004

### 7. Payment Service

```
src/Services/Payment/
├── ECommerce.Payment.API/
│   ├── Controllers/
│   │   ├── PaymentsController.cs        # Payment operations
│   │   └── WebhookController.cs         # Payment webhooks
│   └── Program.cs
│
├── ECommerce.Payment.Application/
│   ├── DTOs/
│   │   ├── Payment/                     # Payment DTOs
│   │   └── Refund/                      # Refund DTOs
│   ├── Services/
│   │   ├── PaymentService.cs            # Payment logic
│   │   └── RefundService.cs             # Refund logic
│   └── Validators/
│       └── ProcessPaymentValidator.cs
│
├── ECommerce.Payment.Domain/
│   ├── Entities/
│   │   ├── Payment.cs                   # Payment entity
│   │   ├── Refund.cs                    # Refund entity
│   │   └── Transaction.cs               # Transaction log
│   ├── Enums/
│   │   ├── PaymentStatus.cs
│   │   └── PaymentMethod.cs
│   └── Interfaces/
│       └── IPaymentRepository.cs
│
└── ECommerce.Payment.Infrastructure/
    ├── Data/
    │   ├── PaymentDbContext.cs          # EF Core DbContext
    │   └── Configurations/
    ├── Repositories/
    │   └── PaymentRepository.cs
    ├── Gateways/
    │   ├── PayOSGateway.cs              # PayOS integration
    │   └── StripeGateway.cs             # Stripe integration
    ├── GrpcClients/
    │   └── OrderGrpcClient.cs           # gRPC client to Order Service
    └── Migrations/
```

**Database:** PostgreSQL  
**Port:** 5005

### 8. Notification Service

```
src/Services/Notification/
├── ECommerce.Notification.API/
│   ├── Controllers/
│   │   ├── EmailController.cs           # Email endpoints
│   │   └── NotificationsController.cs   # Notification management
│   └── Program.cs
│
├── ECommerce.Notification.Application/
│   ├── DTOs/
│   │   ├── Email/                       # Email DTOs
│   │   └── Notification/                # Notification DTOs
│   ├── Services/
│   │   ├── EmailService.cs              # Email sending logic
│   │   └── NotificationService.cs       # Notification logic
│   ├── EventHandlers/
│   │   ├── OrderCreatedHandler.cs       # Handle order events
│   │   ├── PaymentCompletedHandler.cs   # Handle payment events
│   │   └── UserRegisteredHandler.cs     # Handle user events
│   └── Interfaces/
│       └── IEmailService.cs
│
├── ECommerce.Notification.Domain/
│   ├── Entities/
│   │   ├── Notification.cs              # Notification entity
│   │   ├── EmailLog.cs                  # Email log
│   │   └── NotificationTemplate.cs      # Email templates
│   ├── Enums/
│   │   ├── NotificationType.cs
│   │   └── NotificationStatus.cs
│   └── Interfaces/
│       └── INotificationRepository.cs
│
└── ECommerce.Notification.Infrastructure/
    ├── Data/
    │   └── NotificationDbContext.cs     # MongoDB context
    ├── Repositories/
    │   └── NotificationRepository.cs
    └── Services/
        └── SmtpEmailService.cs          # SMTP email service
```

**Database:** MongoDB  
**Port:** 5006

### 9. BuildingBlocks (Shared Libraries)

```
src/BuildingBlocks/
├── ECommerce.Common/
│   ├── Constants/
│   │   └── AppConstants.cs              # Application constants
│   ├── Exceptions/                       # Custom exceptions
│   │   ├── BaseException.cs
│   │   ├── NotFoundException.cs
│   │   ├── ValidationException.cs
│   │   └── UnauthorizedException.cs
│   ├── Extensions/                       # Extension methods
│   │   ├── StringExtensions.cs
│   │   └── DateTimeExtensions.cs
│   └── Models/                           # Common models
│       ├── ApiResponse.cs
│       ├── ErrorResponse.cs
│       └── PagedResult.cs
│
├── ECommerce.EventBus/
│   ├── Abstractions/
│   │   ├── IEventBus.cs                 # Event bus interface
│   │   └── IntegrationEvent.cs          # Base event class
│   ├── Events/                           # Event definitions
│   │   ├── OrderCreatedEvent.cs
│   │   ├── PaymentCompletedEvent.cs
│   │   └── UserRegisteredEvent.cs
│   ├── InMemory/
│   │   └── InMemoryEventBus.cs          # In-memory implementation
│   └── RabbitMQ/
│       ├── RabbitMQEventBus.cs          # RabbitMQ implementation
│       └── RabbitMQExtensions.cs
│
├── ECommerce.Logging/
│   └── LoggingExtensions.cs             # Serilog configuration
│
└── ECommerce.Shared.Abstractions/
    ├── Entities/
    │   ├── BaseEntity.cs                # Base entity class
    │   ├── AuditableEntity.cs           # Auditable entity
    │   ├── IAuditableEntity.cs
    │   └── ISoftDeletable.cs
    ├── Repositories/
    │   ├── IRepository.cs               # Generic repository
    │   └── IUnitOfWork.cs               # Unit of work pattern
    └── Interceptors/
        └── AuditableEntityInterceptor.cs # EF Core interceptor
```

## Docker Configuration

```
docker/
├── docker-compose.yml                    # Application services
├── docker-compose.override.yml           # Development overrides
├── .env                                  # Environment variables
├── start.ps1                            # Start script (Windows)
├── stop.ps1                             # Stop script (Windows)
└── README.md                            # Docker documentation
```

## Documentation

```
docs/
├── api/
│   └── api-document.md                  # API documentation
├── architecture/
│   └── (future architecture diagrams)
├── core/
│   ├── architecture.md                  # System architecture
│   ├── database-document.md             # Database design
│   └── project-structure.md             # This file
├── deployment/
│   ├── deployment.md                    # Deployment guide
│   ├── hosting-options.md               # Hosting comparison
│   ├── docker-single-server.md          # Docker deployment
│   ├── render-deployment.md             # Render.com guide
│   └── free-hosting-comparison.md       # Free hosting options
├── services/
│   └── (future service-specific docs)
└── tools/
    └── CODERABBIT_SETUP.md              # CodeRabbit configuration
```

## Naming Conventions

### Projects
- **API Layer**: `ECommerce.{ServiceName}.API`
- **Application Layer**: `ECommerce.{ServiceName}.Application`
- **Domain Layer**: `ECommerce.{ServiceName}.Domain`
- **Infrastructure Layer**: `ECommerce.{ServiceName}.Infrastructure`

### Namespaces
```csharp
// API Layer
namespace ECommerce.User.API.Controllers;

// Application Layer
namespace ECommerce.User.Application.Services;
namespace ECommerce.User.Application.DTOs.Auth;

// Domain Layer
namespace ECommerce.User.Domain.Entities;
namespace ECommerce.User.Domain.Interfaces;

// Infrastructure Layer
namespace ECommerce.User.Infrastructure.Data;
namespace ECommerce.User.Infrastructure.Repositories;
```

### Files
- **Controllers**: `{EntityName}Controller.cs` (e.g., `UsersController.cs`)
- **Services**: `{EntityName}Service.cs` (e.g., `UserService.cs`)
- **Repositories**: `{EntityName}Repository.cs` (e.g., `UserRepository.cs`)
- **DTOs**: `{Action}{EntityName}Dto.cs` (e.g., `CreateUserDto.cs`)
- **Validators**: `{Dto}Validator.cs` (e.g., `CreateUserDtoValidator.cs`)

## Dependencies Between Services

```
┌─────────────┐
│ API Gateway │
└──────┬──────┘
       │
       ├──────────────────────────────────────┐
       │                                      │
       ↓                                      ↓
┌──────────────┐                      ┌──────────────┐
│ User Service │←─────gRPC────────────│ Order Service│
└──────────────┘                      └──────┬───────┘
                                             │
       ┌─────────────────────────────────────┤
       │                                     │
       ↓                                     ↓
┌──────────────┐                      ┌──────────────┐
│Product Service│←─────gRPC────────────│Payment Service│
└──────┬───────┘                      └──────────────┘
       │
       ↓
┌──────────────┐
│  Cart Service│
└──────────────┘
       │
       ↓
┌──────────────┐
│Notification  │
│   Service    │
└──────────────┘
```

## Port Assignments

| Service | HTTP Port | HTTPS Port | gRPC Port |
|---------|-----------|------------|-----------|
| API Gateway | 5000 | 5443 | - |
| User Service | 5001 | 5444 | 5101 |
| Product Service | 5002 | 5445 | 5102 |
| Shopping Cart Service | 5003 | 5446 | 5103 |
| Order Service | 5004 | 5447 | 5104 |
| Payment Service | 5005 | 5448 | 5105 |
| Notification Service | 5006 | 5449 | 5106 |

## Database Connections

| Service | Database | Connection String |
|---------|----------|-------------------|
| User Service | PostgreSQL | `Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=xxx` |
| Product Service | MongoDB | `mongodb://localhost:27017/ProductDb` |
| Shopping Cart Service | Redis | `localhost:6379` |
| Order Service | PostgreSQL | `Host=localhost;Port=5432;Database=OrderDb;Username=postgres;Password=xxx` |
| Payment Service | PostgreSQL | `Host=localhost;Port=5432;Database=PaymentDb;Username=postgres;Password=xxx` |
| Notification Service | MongoDB | `mongodb://localhost:27017/NotificationDb` |

## Development Workflow

1. **Tạo feature branch**
   ```bash
   git checkout -b feature/new-feature
   ```

2. **Phát triển trong service cụ thể**
   ```bash
   cd src/Services/Users/ECommerce.User.API
   dotnet watch run
   ```

3. **Chạy tests**
   ```bash
   dotnet test
   ```

4. **Build toàn bộ solution**
   ```bash
   dotnet build
   ```

5. **Commit và push**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   git push origin feature/new-feature
   ```

## Best Practices

### 1. Clean Architecture
- Mỗi layer chỉ phụ thuộc vào layer bên trong
- Domain layer không phụ thuộc vào bất kỳ layer nào
- Infrastructure layer implement interfaces từ Domain

### 2. Dependency Injection
- Đăng ký services trong `Program.cs`
- Sử dụng constructor injection
- Tránh service locator pattern

### 3. Error Handling
- Sử dụng custom exceptions
- Global exception middleware
- Trả về consistent error responses

### 4. Validation
- FluentValidation cho input validation
- Domain validation trong entities
- Business rules trong domain services

### 5. Logging
- Structured logging với Serilog
- Log tất cả errors
- Sử dụng correlation IDs

## Tài Liệu Tham Khảo

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microservices Patterns](https://microservices.io/patterns/)
- [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)

---

**Phiên Bản**: 1.0  
**Cập Nhật Lần Cuối**: Tháng 12 năm 2025  
**Người Duy Trì**: Development Team
