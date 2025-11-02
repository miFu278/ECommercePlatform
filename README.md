# E-Commerce Microservices Platform

A scalable, cloud-native e-commerce platform built with .NET 9 microservices architecture.

## 🚀 Features

- **Microservices Architecture** - Independent, scalable services
- **Clean Architecture** - Separation of concerns, testable code
- **Event-Driven** - Asynchronous communication via RabbitMQ
- **Polyglot Persistence** - PostgreSQL, MongoDB, Redis
- **Docker Support** - Containerized services
- **API Gateway** - Centralized routing with Ocelot
- **Authentication** - JWT-based authentication
- **Monitoring** - Structured logging with Serilog

## 📋 Services

| Service | Description | Database | Status |
|---------|-------------|----------|--------|
| **User Service** | User management, authentication | PostgreSQL | 🚧 In Progress |
| **Product Catalog** | Product management, search | MongoDB | 📝 Planned |
| **Shopping Cart** | Cart management | Redis | 📝 Planned |
| **Order Service** | Order processing | PostgreSQL | 📝 Planned |
| **Payment Service** | Payment processing | PostgreSQL | 📝 Planned |
| **Notification Service** | Email, SMS notifications | MongoDB | 📝 Planned |

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                 │
└────────┬────────────────────────────────────────────────┘
         │
    ┌────┴────┬────────┬────────┬────────┬────────┐
    ↓         ↓        ↓        ↓        ↓        ↓
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│  User  │ │Product │ │  Cart  │ │ Order  │ │Payment │ │ Notify │
│Service │ │Service │ │Service │ │Service │ │Service │ │Service │
└───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘
    │          │          │          │          │          │
    ↓          ↓          ↓          ↓          ↓          ↓
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│Postgres│ │MongoDB │ │ Redis  │ │Postgres│ │Postgres│ │MongoDB │
└────────┘ └────────┘ └────────┘ └────────┘ └────────┘ └────────┘
         │
    ┌────┴────┐
    ↓         ↓
┌────────┐ ┌────────┐
│RabbitMQ│ │  Seq   │
└────────┘ └────────┘
```

## 🛠️ Tech Stack

### Core
- **.NET 9** - Framework
- **C# 12** - Language
- **ASP.NET Core** - Web API

### Databases
- **PostgreSQL** - User, Order, Payment services
- **MongoDB** - Product, Notification services
- **Redis** - Shopping cart, caching

### Communication
- **REST APIs** - Synchronous communication
- **RabbitMQ** - Asynchronous messaging
- **MassTransit** - Message bus abstraction

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Local development
- **Kubernetes** - Production orchestration (planned)

### Libraries
- **Entity Framework Core** - ORM
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/miFu278/ECommercePlatform.git
cd ECommercePlatform
```

### 2. Start Infrastructure Services

```bash
cd docker
docker-compose -f docker-compose.infrastructure.yml up -d
```

This starts:
- PostgreSQL (port 5432)
- MongoDB (port 27017)
- Redis (port 6379)
- RabbitMQ (ports 5672, 15672)

### 3. Run Migrations

```bash
cd src/Services/Users/ECommerce.User.API
dotnet ef database update --project ../ECommerce.User.Infrastructure
```

### 4. Run Services

```bash
# User Service
cd src/Services/Users/ECommerce.User.API
dotnet run
```

### 5. Test API

Open browser: http://localhost:5000/swagger

## 📁 Project Structure

```
ECommercePlatform/
├── src/
│   ├── ApiGateway/              # API Gateway
│   ├── Services/                # Microservices
│   │   ├── Users/              # User Service
│   │   │   ├── ECommerce.User.API/
│   │   │   ├── ECommerce.User.Application/
│   │   │   ├── ECommerce.User.Domain/
│   │   │   └── ECommerce.User.Infrastructure/
│   │   ├── Product/            # Product Service
│   │   ├── ShoppingCart/       # Cart Service
│   │   ├── Order/              # Order Service
│   │   ├── Payment/            # Payment Service
│   │   └── Notification/       # Notification Service
│   └── BuildingBlocks/         # Shared libraries
│       ├── ECommerce.Common/
│       ├── ECommerce.EventBus/
│       └── ECommerce.Logging/
├── tests/                       # Unit & Integration tests
├── docker/                      # Docker configurations
├── docs/                        # Documentation
└── k8s/                         # Kubernetes manifests
```

## 📚 Documentation

- [Architecture](docs/architecture.md) - System architecture and design
- [API Documentation](docs/api-document.md) - API endpoints and examples
- [Database Design](docs/database-document.md) - Database schemas
- [Deployment Guide](docs/deployment.md) - Deployment instructions
- [Hosting Options](docs/hosting-options.md) - Cloud hosting guide
- [Docker Setup](docker/README.md) - Docker configuration

## 🔧 Development

### Build Solution

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Code Style

This project follows:
- Clean Architecture principles
- SOLID principles
- Domain-Driven Design (DDD)
- Repository pattern
- CQRS pattern (planned)

## 🐳 Docker

### Start All Services

```bash
cd docker
.\start.ps1
```

### Stop All Services

```bash
.\stop.ps1
```

### View Logs

```bash
docker-compose -f docker-compose.infrastructure.yml logs -f
```

## 🌐 API Endpoints

### User Service

```
POST   /api/v1/users/register      - Register new user
POST   /api/v1/users/login         - Login
POST   /api/v1/users/refresh       - Refresh token
GET    /api/v1/users/profile       - Get user profile
PUT    /api/v1/users/profile       - Update profile
POST   /api/v1/users/change-password - Change password
```

See [API Documentation](docs/api-document.md) for complete API reference.

## 🚀 Deployment

### Development
- Local Docker Compose

### Staging/Production
- Azure Container Apps (Recommended)
- Azure Kubernetes Service (AKS)
- AWS ECS/EKS
- Railway (Budget-friendly)

See [Hosting Options](docs/hosting-options.md) for detailed deployment guides.

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines first.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Minh Phuc** - *Initial work*

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Microservices Patterns by Chris Richardson
- .NET Microservices Architecture Guide by Microsoft

## 📞 Contact

- Email: phucttm.dev@gmail.com
- GitHub: [@miFu278](https://github.com/miFu278)

---

**Status**: 🚧 Work in Progress  
**Version**: 0.1.0  
**Last Updated**: November 2025
