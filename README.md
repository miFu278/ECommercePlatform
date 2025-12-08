# Nền Tảng Thương Mại Điện Tử - Kiến Trúc Microservices

Nền tảng thương mại điện tử có khả năng mở rộng cao, được xây dựng với kiến trúc microservices sử dụng .NET 9.

## 🚀 Tính Năng Chính

- **Kiến Trúc Microservices** - Các dịch vụ độc lập, có khả năng mở rộng
- **Clean Architecture** - Tách biệt các mối quan tâm, code dễ kiểm thử
- **Event-Driven** - Giao tiếp bất đồng bộ qua RabbitMQ
- **Polyglot Persistence** - PostgreSQL, MongoDB, Redis
- **Docker Support** - Các dịch vụ được container hóa
- **API Gateway** - Định tuyến tập trung với Ocelot
- **Authentication** - Xác thực dựa trên JWT
- **Monitoring** - Logging có cấu trúc với Serilog

## 📋 Các Dịch Vụ

| Dịch Vụ | Mô Tả | Cơ Sở Dữ Liệu | Trạng Thái |
|---------|-------|---------------|------------|
| **User Service** | Quản lý người dùng, xác thực | PostgreSQL | ✅ Hoàn thiện (95%) |
| **Product Catalog** | Quản lý sản phẩm, tìm kiếm | MongoDB | ✅ Hoàn thiện (95%) |
| **Shopping Cart** | Quản lý giỏ hàng | Redis | ✅ Hoàn thiện (90%) |
| **Order Service** | Xử lý đơn hàng | PostgreSQL | ✅ Hoàn thiện (92%) |
| **Payment Service** | Xử lý thanh toán | PostgreSQL | ✅ Hoàn thiện (90%) |
| **Notification Service** | Thông báo Email, SMS | MongoDB | ✅ Hoàn thiện (88%) |

## 🏗️ Kiến Trúc

```
┌─────────────────────────────────────────────────────────┐
│                  API Gateway (Ocelot)                    │
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

## 🛠️ Công Nghệ Sử Dụng

### Core
- **.NET 9** - Framework
- **C# 12** - Ngôn ngữ lập trình
- **ASP.NET Core** - Web API

### Cơ Sở Dữ Liệu
- **PostgreSQL** - User, Order, Payment services
- **MongoDB** - Product, Notification services
- **Redis** - Shopping cart, caching

### Giao Tiếp
- **REST APIs** - Giao tiếp đồng bộ
- **gRPC** - Giao tiếp nội bộ hiệu suất cao
- **RabbitMQ** - Message broker
- **MassTransit** - Message bus abstraction

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Local development
- **Kubernetes** - Production orchestration

### Thư Viện
- **Entity Framework Core** - ORM
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

## 🚀 Bắt Đầu Nhanh

### Yêu Cầu

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone https://github.com/miFu278/ECommercePlatform.git
cd ECommercePlatform
```

### 2. Khởi Động Infrastructure Services

```bash
cd docker
docker-compose up -d
```

Điều này sẽ khởi động:
- PostgreSQL (port 5432)
- MongoDB (port 27017)
- Redis (port 6379)
- RabbitMQ (ports 5672, 15672)
- Seq (port 5341)

### 3. Chạy Migrations

```bash
cd src/Services/Users/ECommerce.User.API
dotnet ef database update --project ../ECommerce.User.Infrastructure

cd ../../Order/ECommerce.Order.API
dotnet ef database update --project ../ECommerce.Order.Infrastructure

cd ../../Payment/ECommerce.Payment.API
dotnet ef database update --project ../ECommerce.Payment.Infrastructure
```

### 4. Chạy Các Dịch Vụ

```bash
# API Gateway
cd src/ApiGateway
dotnet run

# User Service
cd src/Services/Users/ECommerce.User.API
dotnet run

# Product Service
cd src/Services/Product/ECommerce.Product.API
dotnet run

# Shopping Cart Service
cd src/Services/ShoppingCart/ECommerce.ShoppingCart.API
dotnet run

# Order Service
cd src/Services/Order/ECommerce.Order.API
dotnet run

# Payment Service
cd src/Services/Payment/ECommerce.Payment.API
dotnet run

# Notification Service
cd src/Services/Notification/ECommerce.Notification.API
dotnet run
```

### 5. Kiểm Tra API

Mở trình duyệt: http://localhost:5000/swagger

## 📁 Cấu Trúc Dự Án

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
│       ├── ECommerce.Logging/
│       └── ECommerce.Shared.Abstractions/
├── docker/                      # Docker configurations
└── docs/                        # Documentation
    ├── api/                     # API documentation
    ├── architecture/            # Architecture docs
    ├── core/                    # Core documentation
    ├── deployment/              # Deployment guides
    ├── services/                # Service-specific docs
    └── tools/                   # Tool configurations
```

## 📚 Tài Liệu

### Tài Liệu Cốt Lõi
- [Kiến Trúc Hệ Thống](docs/core/architecture.md) - Kiến trúc và design patterns
- [Thiết Kế Cơ Sở Dữ Liệu](docs/core/database-document.md) - Database schemas cho tất cả services
- [Cấu Trúc Dự Án](docs/core/project-structure.md) - Tổ chức solution

### API & Deployment
- [Tài Liệu API](docs/api/api-document.md) - Tham khảo API đầy đủ
- [Hướng Dẫn Deployment](docs/deployment/deployment.md) - Local, Docker, Kubernetes
- [Tùy Chọn Hosting](docs/deployment/hosting-options.md) - So sánh cloud hosting

### Công Cụ & Hướng Dẫn
- [Cài Đặt CodeRabbit](docs/tools/CODERABBIT_SETUP.md) - AI code review
- [Báo Cáo Hoàn Thiện](COMPLETENESS_REPORT.md) - Trạng thái dự án
- [Hướng Dẫn Đóng Góp](CONTRIBUTING.md) - Cách đóng góp

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

Dự án này tuân theo:
- Clean Architecture principles
- SOLID principles
- Domain-Driven Design (DDD)
- Repository pattern
- CQRS pattern

## 🐳 Docker

### Khởi Động Tất Cả Services

```bash
cd docker
docker-compose up -d
```

### Dừng Tất Cả Services

```bash
docker-compose down
```

### Xem Logs

```bash
docker-compose logs -f
```

## 🌐 API Endpoints

### User Service

```
POST   /api/v1/users/register      - Đăng ký người dùng mới
POST   /api/v1/users/login         - Đăng nhập
POST   /api/v1/users/refresh       - Refresh token
GET    /api/v1/users/profile       - Lấy thông tin profile
PUT    /api/v1/users/profile       - Cập nhật profile
POST   /api/v1/users/change-password - Đổi mật khẩu
```

### Product Service

```
GET    /api/v1/products            - Lấy danh sách sản phẩm
GET    /api/v1/products/{id}       - Lấy chi tiết sản phẩm
POST   /api/v1/products            - Tạo sản phẩm mới (Admin)
PUT    /api/v1/products/{id}       - Cập nhật sản phẩm (Admin)
DELETE /api/v1/products/{id}       - Xóa sản phẩm (Admin)
GET    /api/v1/categories          - Lấy danh mục
```

### Shopping Cart Service

```
GET    /api/v1/cart                - Lấy giỏ hàng
POST   /api/v1/cart/items          - Thêm sản phẩm vào giỏ
PUT    /api/v1/cart/items/{id}     - Cập nhật số lượng
DELETE /api/v1/cart/items/{id}     - Xóa sản phẩm khỏi giỏ
DELETE /api/v1/cart                - Xóa toàn bộ giỏ hàng
```

### Order Service

```
POST   /api/v1/orders              - Tạo đơn hàng
GET    /api/v1/orders/{id}         - Lấy chi tiết đơn hàng
GET    /api/v1/orders              - Lấy danh sách đơn hàng
POST   /api/v1/orders/{id}/cancel  - Hủy đơn hàng
```

### Payment Service

```
POST   /api/v1/payments/process    - Xử lý thanh toán
GET    /api/v1/payments/{id}       - Lấy thông tin thanh toán
POST   /api/v1/payments/{id}/refund - Hoàn tiền
```

Xem [Tài Liệu API](docs/api/api-document.md) để biết thêm chi tiết.

## 🚀 Deployment

### Development
- Local Docker Compose

### Staging/Production
- Azure Container Apps (Khuyến nghị)
- Azure Kubernetes Service (AKS)
- AWS ECS/EKS
- Railway (Budget-friendly)

Xem [Tùy Chọn Hosting](docs/deployment/hosting-options.md) để biết hướng dẫn deployment chi tiết.

## 🤝 Đóng Góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng đọc [hướng dẫn đóng góp](CONTRIBUTING.md) trước.

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📝 License

Dự án này được cấp phép theo giấy phép MIT - xem file [LICENSE](LICENSE) để biết chi tiết.

## 👥 Tác Giả

- **Minh Phuc** - *Initial work*

## 🙏 Cảm Ơn

- Clean Architecture by Robert C. Martin
- Microservices Patterns by Chris Richardson
- .NET Microservices Architecture Guide by Microsoft

## 📞 Liên Hệ

- Email: phucttm.dev@gmail.com
- GitHub: [@miFu278](https://github.com/miFu278)

---

**Trạng Thái**: 🚧 Đang Phát Triển  
**Phiên Bản**: 0.3.0  
**Cập Nhật Lần Cuối**: Tháng 12 năm 2025
