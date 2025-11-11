# Project Structure

## Overview

E-Commerce Microservices Platform với kiến trúc phân tán, sử dụng Clean Architecture và DDD patterns.

## Architecture

```
┌─────────────┐
│   Clients   │ (Mobile/Web)
└──────┬──────┘
       │ HTTP/REST
       ↓
┌─────────────┐
│ API Gateway │ (Ocelot/YARP)
└──────┬──────┘
       │
       ├─────→ gRPC (Sync Queries)
       └─────→ RabbitMQ (Async Events)
       │
┌──────┴──────────────────────────┐
│        Microservices            │
├─────────────────────────────────┤
│ • User Service                  │
│ • Product Service               │
│ • Order Service                 │
│ • Payment Service               │
│ • Shopping Cart Service         │
│ • Notification Service          │
└─────────────────────────────────┘
```

## Communication Patterns

### 🌐 HTTP/REST
**Usage:** Client ↔ API Gateway only
- External API exposure
- Third-party integrations

### ⚡ gRPC
**Usage:** Synchronous inter-service communication
- API Gateway → Services (queries)
- Service → Service (real-time data needs)
- Examples:
  - Order Service → Product Service (get product details)
  - Order Service → User Service (validate user)
  - Cart Service → Product Service (check stock)

### 🐰 RabbitMQ
**Usage:** Asynchronous event-driven communication
- Fire-and-forget operations
- Event notifications across services
- Examples:
  - OrderCreatedEvent → Product Service (reserve stock)
  - PaymentSuccessEvent → Order Service (update status)
  - OrderShippedEvent → Notification Service (send email)

## Project Structure

### Root Level
```
ECommercePlatform/
├── src/
│   ├── ApiGateway/              # API Gateway (Ocelot/YARP)
│   ├── BuildingBlocks/          # Shared libraries
│   │   ├── Common/              # Common utilities
│   │   ├── EventBus/            # RabbitMQ abstractions
│   │   └── gRPC/                # gRPC shared contracts
│   └── Services/                # Microservices
├── docker/                      # Docker compose files
├── k8s/                         # Kubernetes manifests
├── docs/                        # Documentation
└── tests/                       # Integration tests
```

### Service Structure (Clean Architecture)
```
ECommerce.{Service}/
├── ECommerce.{Service}.API/
│   ├── Controllers/             # REST endpoints
│   ├── Grpc/                    # gRPC services
│   │   ├── Services/            # gRPC service implementations
│   │   └── Protos/              # .proto files
│   ├── EventHandlers/           # RabbitMQ consumers
│   └── Program.cs
├── ECommerce.{Service}.Application/
│   ├── DTOs/
│   ├── Services/
│   ├── Interfaces/
│   ├── Mappings/
│   ├── Validators/
│   └── Events/                  # Event definitions
│       ├── Publishers/          # RabbitMQ publishers
│       └── Consumers/           # RabbitMQ consumers
├── ECommerce.{Service}.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   └── Interfaces/
└── ECommerce.{Service}.Infrastructure/
    ├── Data/
    ├── Repositories/
    └── Services/
```

### BuildingBlocks Structure
```
src/BuildingBlocks/
├── ECommerce.Shared/
│   ├── Authentication/          # JWT utilities
│   ├── Extensions/              # Common extensions
│   └── Models/                  # Shared DTOs
├── ECommerce.EventBus/
│   ├── Abstractions/            # IEventBus interface
│   ├── RabbitMQ/                # RabbitMQ implementation
│   └── Events/                  # Base event classes
└── ECommerce.Grpc.Contracts/
    ├── Product/                 # Product service contracts
    ├── User/                    # User service contracts
    ├── Order/                   # Order service contracts
    └── Shared/                  # Shared messages
```

## Services Overview

### 👤 User Service
- Authentication & Authorization
- User profile management
- JWT token generation
- **gRPC:** GetUser, ValidateUser
- **Events:** UserRegistered, UserUpdated

### 📦 Product Service
- Product catalog management
- Inventory tracking
- **gRPC:** GetProduct, CheckAvailability, SearchProducts
- **Events:** ProductCreated, StockChanged, LowStockAlert

### 🛒 Shopping Cart Service
- Cart management
- **gRPC:** GetCart, AddToCart
- **Events:** CartUpdated, CartAbandoned

### 📋 Order Service
- Order processing
- Order lifecycle management
- **gRPC:** CreateOrder, GetOrder, GetUserOrders
- **Events:** OrderCreated, OrderPaid, OrderShipped, OrderCompleted, OrderCancelled

### 💳 Payment Service
- Payment processing
- Payment gateway integration
- **gRPC:** ProcessPayment, GetPaymentStatus
- **Events:** PaymentSuccess, PaymentFailed, RefundProcessed

### 📧 Notification Service
- Email notifications
- SMS notifications
- Push notifications
- **Events Consumed:** All events that require notifications

## Configuration Files

### Public (Committed)
```
appsettings.json              # Template configuration
appsettings.Example.json      # Example with placeholders
CONFIGURATION.md              # Setup instructions
```

### Private (Gitignored)
```
appsettings.Development.json  # Local development config
appsettings.Production.json   # Production config
*.http                        # Test requests
```

## Documentation

### Public Documentation
```
docs/
├── api/                      # API documentation
├── services/                 # Service-specific docs
├── guides/                   # Implementation guides
└── deployment/               # Deployment guides
```

### GitHub
```
.github/
├── workflows/
│   └── coderabbit.yml        # CodeRabbit workflow
└── pull_request_template.md  # PR template
```

## Inter-Service Communication Examples

### Example 1: Create Order Flow

```
Client → API Gateway (REST)
  ↓
API Gateway → Order Service (gRPC: CreateOrder)
  ↓
Order Service → Product Service (gRPC: GetProduct, CheckStock)
Order Service → User Service (gRPC: ValidateUser)
  ↓
Order Service saves to DB
  ↓
Order Service → RabbitMQ (Publish: OrderCreatedEvent)
  ↓
  ├─→ Product Service (Reserve stock)
  ├─→ Notification Service (Send confirmation email)
  └─→ Analytics Service (Track order)
```

### Example 2: Payment Success Flow

```
Payment Service → RabbitMQ (Publish: PaymentSuccessEvent)
  ↓
  ├─→ Order Service (Update status to Paid)
  ├─→ Notification Service (Send receipt)
  └─→ Loyalty Service (Add points)
```

### Example 3: JWT Authentication Flow

```
1. User Login
   Client → API Gateway → Auth Service
   ↓
   Return JWT token

2. Authenticated Request
   Client → API Gateway (with JWT in header)
   ↓
   API Gateway validates JWT
   ↓
   Extract user info (userId, roles)
   ↓
   API Gateway → Service (gRPC with metadata)
   
   Metadata:
   {
     "user-id": "123",
     "user-email": "user@email.com",
     "user-roles": "Customer,Premium"
   }
```

## Technology Stack

### Communication
- **REST API:** ASP.NET Core Web API
- **gRPC:** Grpc.AspNetCore, Grpc.Net.Client
- **Message Broker:** RabbitMQ with MassTransit
- **API Gateway:** Ocelot or YARP

### Authentication
- **JWT:** Microsoft.AspNetCore.Authentication.JwtBearer
- **Validation:** Centralized at API Gateway

### Databases
- **User Service:** PostgreSQL
- **Product Service:** MongoDB
- **Order Service:** PostgreSQL
- **Cart Service:** Redis

### Infrastructure
- **Containerization:** Docker
- **Orchestration:** Kubernetes
- **Service Discovery:** Consul (optional)
- **Monitoring:** Prometheus + Grafana

## Quick Reference

### Setup New Service
```bash
# 1. Create service structure
dotnet new webapi -n ECommerce.{Service}.API
dotnet new classlib -n ECommerce.{Service}.Application
dotnet new classlib -n ECommerce.{Service}.Domain
dotnet new classlib -n ECommerce.{Service}.Infrastructure

# 2. Add gRPC support
dotnet add package Grpc.AspNetCore

# 3. Add RabbitMQ support
dotnet add package MassTransit.RabbitMQ

# 4. Add shared libraries
dotnet add reference ../../BuildingBlocks/ECommerce.Shared
```

### Add gRPC Service
```bash
# 1. Create .proto file in Protos/
# 2. Add to .csproj
<Protobuf Include="Protos\{service}.proto" GrpcServices="Server" />

# 3. Implement service
# 4. Register in Program.cs
app.MapGrpcService<{Service}GrpcService>();
```

### Add RabbitMQ Event
```bash
# 1. Define event in Application/Events/
# 2. Create consumer
# 3. Register in Program.cs
builder.Services.AddMassTransit(x => {
    x.AddConsumer<{Event}Consumer>();
    x.UsingRabbitMq((context, cfg) => {
        cfg.ConfigureEndpoints(context);
    });
});
```

### Before Commit
```bash
# Verify no sensitive files
git status

# Should NOT see:
# - appsettings.Development.json
# - appsettings.Production.json
# - *.http files with real data
```

### Run Infrastructure
```bash
# Start RabbitMQ, Redis, Databases
cd docker
docker-compose -f docker-compose.infrastructure.yml up -d
```

### Run Services
```bash
# Terminal 1: API Gateway
cd src/ApiGateway
dotnet run

# Terminal 2: User Service
cd src/Services/Users/ECommerce.User.API
dotnet run

# Terminal 3: Product Service
cd src/Services/Product/ECommerce.Product.API
dotnet run

# Terminal 4: Order Service
cd src/Services/Order/ECommerce.Order.API
dotnet run
```

## Best Practices

### Communication
- ✅ Use gRPC for synchronous queries that need immediate response
- ✅ Use RabbitMQ for asynchronous events and fire-and-forget operations
- ✅ Use HTTP/REST only for external API exposure
- ✅ Always forward correlation-id for distributed tracing

### Security
- ✅ Validate JWT at API Gateway only
- ✅ Forward user context via gRPC metadata
- ✅ Never store secrets in code or appsettings.json
- ✅ Use environment variables or Azure Key Vault

### Error Handling
- ✅ Use RpcException for gRPC errors
- ✅ Implement retry policies for RabbitMQ consumers
- ✅ Log correlation-id for tracing across services
- ✅ Return meaningful error messages to clients

### Performance
- ✅ Use Redis for caching frequently accessed data
- ✅ Implement circuit breaker pattern for service calls
- ✅ Use connection pooling for databases
- ✅ Monitor service health and metrics

---

**Keep it simple, scalable, and maintainable!**
