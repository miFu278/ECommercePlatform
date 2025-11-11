# E-Commerce Microservices Platform - Architecture Documentation

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Principles](#architecture-principles)
3. [Tech Stack](#tech-stack)
4. [Microservices Overview](#microservices-overview)
5. [Communication Patterns](#communication-patterns)
6. [Data Management](#data-management)
7. [Security Architecture](#security-architecture)
8. [Infrastructure Components](#infrastructure-components)
9. [Deployment Architecture](#deployment-architecture)
10. [Development Guidelines](#development-guidelines)

---

## System Overview

### Purpose
A scalable, cloud-native e-commerce platform built with microservices architecture using .NET 8, designed to handle high traffic, ensure high availability, and provide seamless shopping experience.

### Key Features
- User registration and authentication
- Product catalog management
- Shopping cart functionality
- Order processing
- Payment integration (Stripe, PayPal)
- Real-time notifications
- Order tracking
- Inventory management

### Architecture Style
- **Pattern**: Microservices Architecture
- **Communication**: REST APIs + Message-based (async)
- **Data**: Database per service (Polyglot Persistence)
- **Deployment**: Containerized with Docker/Kubernetes

---

## Architecture Principles

### 1. Single Responsibility
Each microservice owns a specific business capability and operates independently.

### 2. Decentralized Data Management
Each service manages its own database - no shared databases between services.

### 3. Failure Isolation
Services are designed to handle failures gracefully without cascading effects.

### 4. API First Design
All services expose well-defined APIs with versioning support.

### 5. Observability
Comprehensive logging, monitoring, and distributed tracing across all services.

### 6. Infrastructure as Code
All infrastructure components defined and managed through code.

---

## Tech Stack

### Core Framework
- **.NET 8** - Main development framework
- **ASP.NET Core** - Web API framework
- **C# 12** - Programming language

### API Gateway
- **Ocelot** - Primary API Gateway
- Alternative: **YARP** (Yet Another Reverse Proxy)

### Communication
- **REST APIs** - Synchronous HTTP communication
- **gRPC** - High-performance internal communication
- **MassTransit** - Message abstraction layer
- **RabbitMQ** - Message broker
- **SignalR** - Real-time bidirectional communication

### Databases
| Service | Database | Reason |
|---------|----------|--------|
| User Service | PostgreSQL | Relational data, ACID compliance |
| Product Catalog | MongoDB | Flexible schema, read-heavy |
| Shopping Cart | Redis | Fast in-memory, session data |
| Order Service | PostgreSQL | Transactional integrity |
| Payment Service | PostgreSQL | Financial data, audit trail |

### Authentication & Authorization
- **Duende IdentityServer** - OAuth 2.0/OpenID Connect
- **JWT** - Token-based authentication
- **ASP.NET Core Identity** - User management

### Observability Stack
- **Serilog** - Structured logging
- **Seq** - Log aggregation (development)
- **ELK Stack** - Production logging
- **Prometheus** - Metrics collection
- **Grafana** - Metrics visualization
- **Jaeger** - Distributed tracing

### Testing
- **xUnit** - Unit testing
- **Moq** - Mocking framework
- **FluentAssertions** - Readable assertions
- **Testcontainers** - Integration testing

### DevOps
- **Docker** - Containerization
- **Kubernetes** - Container orchestration
- **Helm** - Kubernetes package manager
- **GitHub Actions** - CI/CD pipelines

---

## Microservices Overview

### 1. User Service
**Responsibility**: User management, authentication, authorization

**Key Features**:
- User registration
- Login/Logout
- Profile management
- Password reset
- Email verification
- Role-based access control

**Technology**:
- Database: PostgreSQL
- Cache: Redis (session management)
- Authentication: JWT + IdentityServer

**API Endpoints**:
```
POST   /api/v1/users/register
POST   /api/v1/users/login
GET    /api/v1/users/profile
PUT    /api/v1/users/profile
POST   /api/v1/users/change-password
POST   /api/v1/users/forgot-password
```

---

### 2. Product Catalog Service
**Responsibility**: Product information management

**Key Features**:
- Product CRUD operations
- Category management
- Inventory tracking
- Product search and filtering
- Price management
- Product images

**Technology**:
- Database: MongoDB (flexible schema)
- Search: Elasticsearch (optional)
- Cache: Redis (frequently accessed products)

**API Endpoints**:
```
GET    /api/v1/products
GET    /api/v1/products/{id}
POST   /api/v1/products
PUT    /api/v1/products/{id}
DELETE /api/v1/products/{id}
GET    /api/v1/products/search?q={query}
GET    /api/v1/categories
```

---

### 3. Shopping Cart Service
**Responsibility**: Shopping cart management

**Key Features**:
- Add items to cart
- Remove items from cart
- Update quantities
- Calculate totals
- Apply discount codes
- Cart persistence

**Technology**:
- Database: Redis (fast, in-memory)
- Cache: Integrated with Redis

**API Endpoints**:
```
GET    /api/v1/cart/{userId}
POST   /api/v1/cart/items
PUT    /api/v1/cart/items/{itemId}
DELETE /api/v1/cart/items/{itemId}
DELETE /api/v1/cart/{userId}/clear
POST   /api/v1/cart/apply-coupon
```

---

### 4. Order Service
**Responsibility**: Order processing and management

**Key Features**:
- Create orders
- Order status tracking
- Order history
- Order cancellation
- Shipping management
- Invoice generation

**Technology**:
- Database: PostgreSQL (transactional)
- Event Sourcing: Optional for order history
- Message Bus: RabbitMQ (order events)

**API Endpoints**:
```
POST   /api/v1/orders
GET    /api/v1/orders/{orderId}
GET    /api/v1/orders/user/{userId}
PUT    /api/v1/orders/{orderId}/status
POST   /api/v1/orders/{orderId}/cancel
GET    /api/v1/orders/{orderId}/invoice
```

**Domain Events**:
- OrderCreatedEvent
- OrderPaidEvent
- OrderShippedEvent
- OrderDeliveredEvent
- OrderCancelledEvent

---

### 5. Payment Service
**Responsibility**: Payment processing

**Key Features**:
- Process payments
- Refund handling
- Payment method management
- Transaction history
- Integration with payment gateways

**Technology**:
- Database: PostgreSQL (financial records)
- Payment Gateways: Stripe, PayPal
- PCI DSS Compliance considerations

**API Endpoints**:
```
POST   /api/v1/payments/process
POST   /api/v1/payments/refund
GET    /api/v1/payments/{paymentId}
GET    /api/v1/payments/order/{orderId}
POST   /api/v1/payments/methods
GET    /api/v1/payments/methods/{userId}
```

**Integration Points**:
- Stripe SDK
- PayPal REST API
- Webhook handlers for payment notifications

---

### 6. Notification Service
**Responsibility**: Multi-channel notifications

**Key Features**:
- Email notifications
- SMS notifications (optional)
- Push notifications (optional)
- Notification templates
- Delivery tracking
- Retry mechanism

**Technology**:
- Database: MongoDB (notification logs)
- Email Provider: SendGrid/AWS SES
- SMS Provider: Twilio
- Queue: RabbitMQ (async processing)

**API Endpoints**:
```
POST   /api/v1/notifications/email
POST   /api/v1/notifications/sms
GET    /api/v1/notifications/{userId}/history
```

**Notification Types**:
- Welcome email
- Order confirmation
- Payment confirmation
- Shipping updates
- Password reset
- Promotional campaigns

---

## Communication Patterns

### Synchronous Communication (REST/gRPC)

**When to Use**:
- Client-facing APIs (through API Gateway)
- Real-time data requirements
- Simple request-response patterns

**Example Flow**:
```
Client → API Gateway → User Service (GET user profile)
```

**Implementation**:
```csharp
// Using HttpClient with Polly for resilience
services.AddHttpClient<IProductService, ProductService>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)))
    .AddTransientHttpErrorPolicy(policy => 
        policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

---

### Asynchronous Communication (Message Bus)

**When to Use**:
- Event notifications
- Long-running processes
- Decoupled service interactions
- Eventually consistent data

**Example Flow**:
```
Order Service → RabbitMQ → [Payment Service, Notification Service, Inventory Service]
```

**Message Types**:

1. **Commands** (one consumer)
   - ProcessPaymentCommand
   - CreateOrderCommand

2. **Events** (multiple consumers)
   - OrderCreatedEvent
   - PaymentProcessedEvent
   - ProductStockChangedEvent

**Implementation with MassTransit**:
```csharp
// Publishing an event
await _publishEndpoint.Publish(new OrderCreatedEvent
{
    OrderId = order.Id,
    UserId = order.UserId,
    TotalAmount = order.TotalAmount,
    CreatedAt = DateTime.UtcNow
});

// Consuming an event
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        // Handle the event
        await SendOrderConfirmationEmail(context.Message);
    }
}
```

---

## Data Management

### Database Per Service Pattern

Each microservice owns its database - no direct database access between services.

**Benefits**:
- Service independence
- Technology diversity
- Easier scaling
- Better fault isolation

**Challenges**:
- Data consistency (use Saga pattern)
- Complex queries across services
- Data duplication

### Data Consistency Strategies

#### 1. Saga Pattern (Choreography)
Distributed transactions using events.

**Example: Order Creation Flow**
```
1. Order Service: Create Order (status: Pending) → OrderCreatedEvent
2. Payment Service: Process Payment → PaymentProcessedEvent
3. Inventory Service: Reserve Items → ItemsReservedEvent
4. Notification Service: Send Confirmation
5. Order Service: Update Order (status: Confirmed)
```

If any step fails:
```
Payment Service: Payment Failed → PaymentFailedEvent
Order Service: Cancel Order → OrderCancelledEvent
Inventory Service: Release Reserved Items
Notification Service: Send Cancellation Email
```

#### 2. Event Sourcing (Optional)
Store all changes as events for order history and audit trail.

#### 3. CQRS (Command Query Responsibility Segregation)
Separate read and write models for better performance.

**Write Model**: PostgreSQL (transactional)
**Read Model**: MongoDB/Elasticsearch (optimized queries)

---

## Security Architecture

### Authentication Flow

```
1. User → API Gateway: POST /login (username, password)
2. API Gateway → User Service: Validate credentials
3. User Service → IdentityServer: Request tokens
4. IdentityServer → User: Return JWT Access Token + Refresh Token
5. User → API Gateway: Subsequent requests with Bearer token
6. API Gateway: Validate token, route to services
```

### Authorization

**Role-Based Access Control (RBAC)**:
- Admin: Full access
- Manager: Product management, order management
- Customer: Browse, cart, checkout
- Guest: Browse only

**Implementation**:
```csharp
[Authorize(Roles = "Admin,Manager")]
[HttpPost("products")]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
{
    // Only admins and managers can create products
}
```

### Security Best Practices

1. **API Gateway Security**:
   - Rate limiting
   - Request validation
   - JWT token validation
   - CORS configuration

2. **Service-to-Service Communication**:
   - Mutual TLS (mTLS)
   - API keys for internal services
   - Service mesh (Istio/Linkerd)

3. **Data Protection**:
   - Encrypt sensitive data at rest
   - Use HTTPS for all communications
   - Hash passwords (bcrypt/Argon2)
   - PCI DSS compliance for payment data

4. **Secrets Management**:
   - Azure Key Vault / AWS Secrets Manager
   - Environment variables (not in code)
   - Docker secrets

---

## Infrastructure Components

### API Gateway (Ocelot)

**Responsibilities**:
- Request routing
- Load balancing
- Authentication/Authorization
- Rate limiting
- Request/Response transformation
- Caching
- Logging

**Configuration Example**:
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "product-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    }
  ]
}
```

### Service Discovery (Consul)

**Features**:
- Service registration
- Health checking
- DNS interface
- Key-value storage (configuration)

**Registration**:
```csharp
services.AddConsul();
app.RegisterWithConsul(lifetime, configuration);
```

### Message Broker (RabbitMQ)

**Topology**:
- **Exchanges**: Topic exchange for event routing
- **Queues**: Per service per event type
- **Bindings**: Route events to appropriate queues

**Configuration with MassTransit**:
```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
```

### Distributed Cache (Redis)

**Use Cases**:
- Shopping cart storage
- Session management
- API response caching
- Rate limiting counters

**Implementation**:
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
    options.InstanceName = "ECommerce_";
});
```

---

## Deployment Architecture

### Local Development (Docker Compose)

```yaml
version: '3.8'

services:
  # Infrastructure
  postgres:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  mongodb:
    image: mongo:6
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:80"

  consul:
    image: consul:latest
    ports:
      - "8500:8500"

  # Services
  api-gateway:
    build: ./src/ApiGateway/ECommerce.Gateway
    ports:
      - "5000:80"
    depends_on:
      - user-service
      - product-service

  user-service:
    build: ./src/Services/User/ECommerce.User.API
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=UserDb;Username=postgres;Password=password
      - RabbitMQ__Host=rabbitmq
      - Redis__Connection=redis:6379
    depends_on:
      - postgres
      - rabbitmq
      - redis

  product-service:
    build: ./src/Services/Product/ECommerce.Product.API
    environment:
      - ConnectionStrings__MongoDB=mongodb://mongodb:27017
      - RabbitMQ__Host=rabbitmq
    depends_on:
      - mongodb
      - rabbitmq

volumes:
  postgres_data:
  mongo_data:
```

### Production Deployment (Kubernetes)

**Architecture**:
- Multiple nodes for high availability
- Horizontal Pod Autoscaling (HPA)
- Ingress controller for routing
- Persistent volumes for databases
- ConfigMaps and Secrets for configuration

**Example Deployment**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: product-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: product-service
  template:
    metadata:
      labels:
        app: product-service
    spec:
      containers:
      - name: product-service
        image: ecommerce/product-service:latest
        ports:
        - containerPort: 80
        resources:
          requests:
            memory: "256Mi"
            cpu: "200m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        env:
        - name: ConnectionStrings__MongoDB
          valueFrom:
            secretKeyRef:
              name: mongodb-secret
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: product-service
spec:
  selector:
    app: product-service
  ports:
  - port: 80
    targetPort: 80
  type: ClusterIP
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: product-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: product-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

---

## Development Guidelines

### Code Structure (Clean Architecture)

Each service follows this structure:

```
ECommerce.Service.API/          # Presentation Layer
├── Controllers/
├── Middleware/
├── Filters/
└── Program.cs

ECommerce.Service.Application/  # Application Layer
├── DTOs/
├── Services/
├── Interfaces/
├── Validators/
├── Mappings/
└── Commands/Queries/          # If using CQRS/MediatR

ECommerce.Service.Domain/       # Domain Layer
├── Entities/
├── ValueObjects/
├── Enums/
├── Events/
└── Interfaces/

ECommerce.Service.Infrastructure/ # Infrastructure Layer
├── Data/
│   ├── DbContext.cs
│   └── Migrations/
├── Repositories/
├── Services/                   # External service implementations
└── MessageBus/
```

### Coding Standards

1. **Naming Conventions**:
   - PascalCase for classes, methods, properties
   - camelCase for local variables, parameters
   - Prefix interfaces with 'I' (IUserRepository)

2. **Async/Await**:
   - Use async suffix for async methods
   - Always await async calls

3. **Dependency Injection**:
   - Constructor injection preferred
   - Register services with appropriate lifetime

4. **Error Handling**:
   - Use custom exceptions
   - Global exception middleware
   - Return proper HTTP status codes

5. **Logging**:
   - Log all errors
   - Use structured logging (Serilog)
   - Include correlation IDs

### API Design Guidelines

1. **Versioning**: Use URL versioning (`/api/v1/products`)

2. **HTTP Methods**:
   - GET: Retrieve resources
   - POST: Create resources
   - PUT: Update (full replacement)
   - PATCH: Partial update
   - DELETE: Remove resources

3. **Status Codes**:
   - 200: OK
   - 201: Created
   - 204: No Content
   - 400: Bad Request
   - 401: Unauthorized
   - 403: Forbidden
   - 404: Not Found
   - 409: Conflict
   - 500: Internal Server Error

4. **Response Format**:
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": []
}
```

### Testing Strategy

1. **Unit Tests**: 70%+ coverage
   - Test business logic
   - Mock dependencies
   - Fast execution

2. **Integration Tests**:
   - Test API endpoints
   - Use Testcontainers for databases
   - Test service interactions

3. **E2E Tests**:
   - Test critical user journeys
   - Run in staging environment

4. **Performance Tests**:
   - Load testing with k6/JMeter
   - Benchmark critical paths

### Git Workflow

1. **Branch Strategy**:
   - `main`: Production-ready code
   - `develop`: Integration branch
   - `feature/*`: New features
   - `bugfix/*`: Bug fixes
   - `hotfix/*`: Critical fixes

2. **Commit Messages**:
   ```
   type(scope): subject
   
   body (optional)
   
   footer (optional)
   ```
   Types: feat, fix, docs, style, refactor, test, chore

3. **Pull Requests**:
   - Require code review
   - Pass all tests
   - No merge conflicts

### CI/CD Pipeline

**Build Stage**:
1. Checkout code
2. Restore dependencies
3. Build solution
4. Run unit tests
5. Code quality checks (SonarQube)

**Test Stage**:
1. Run integration tests
2. Generate coverage report

**Deploy Stage**:
1. Build Docker images
2. Push to container registry
3. Deploy to Kubernetes
4. Run smoke tests

---

## Monitoring and Observability

### Health Checks

Implement health checks in each service:

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection)
    .AddRabbitMQ(rabbitConnection);

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Distributed Tracing

Use OpenTelemetry with Jaeger:

```csharp
services.AddOpenTelemetryTracing(builder =>
{
    builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "jaeger";
            options.AgentPort = 6831;
        });
});
```

### Metrics

Expose Prometheus metrics:

```csharp
services.AddPrometheusMetrics();

app.UseHttpMetrics();
app.MapMetrics();
```

### Logging

Structured logging with Serilog:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ProductService")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();
```

---

## Scalability Considerations

### Horizontal Scaling
- Stateless services (scale easily)
- Session stored in Redis
- Use load balancer

### Database Scaling
- Read replicas for read-heavy services
- Connection pooling
- Caching strategy

### Caching Strategy
1. **Client-side**: Browser cache, CDN
2. **API Gateway**: Response caching
3. **Application**: Redis cache
4. **Database**: Query result cache

### Async Processing
- Use message queues for long-running tasks
- Background jobs (Hangfire/Quartz)
- Batch processing

---

## Disaster Recovery

### Backup Strategy
- Database: Daily automated backups
- Configuration: Version controlled
- Secrets: Secure backup

### High Availability
- Multi-zone deployment
- Database replication
- Service redundancy

### Monitoring and Alerts
- Uptime monitoring
- Error rate alerts
- Performance degradation alerts
- Automated incident response

---

## Cost Optimization

1. **Resource Right-sizing**: Monitor and adjust resources
2. **Auto-scaling**: Scale down during low traffic
3. **Spot Instances**: Use for non-critical workloads
4. **Caching**: Reduce database load
5. **CDN**: Reduce bandwidth costs

---

## Future Enhancements

- [ ] Recommendation engine (ML.NET)
- [ ] Real-time inventory updates (SignalR)
- [ ] Multi-currency support
- [ ] Multi-language support (i18n)
- [ ] Advanced analytics dashboard
- [ ] Mobile app (Xamarin/MAUI)
- [ ] GraphQL API (Hot Chocolate)
- [ ] Service mesh (Istio)
- [ ] Event streaming (Kafka)

---

## References

- [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [12-Factor App Methodology](https://12factor.net/)
- [REST API Design Best Practices](https://docs.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- [Microservices Patterns](https://microservices.io/patterns/)

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Maintained By**: Development Team