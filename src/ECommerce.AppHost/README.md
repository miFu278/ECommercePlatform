# ECommerce Aspire AppHost

## Giới thiệu

Project này sử dụng .NET Aspire để orchestrate tất cả microservices trong development environment. Aspire cung cấp:

- **Service Orchestration**: Chạy tất cả services cùng lúc với một lệnh
- **Service Discovery**: Tự động phát hiện và kết nối giữa các services
- **Dashboard**: UI để monitor logs, traces, và metrics
- **Telemetry**: OpenTelemetry tích hợp sẵn

## Yêu cầu

- .NET 9.0 SDK
- Visual Studio 2022 17.9+ hoặc JetBrains Rider 2024.1+
- .NET Aspire workload

### Cài đặt Aspire Workload

```bash
dotnet workload update
dotnet workload install aspire
```

## Managed Services đang sử dụng

Project này sử dụng các managed services, không cần setup local:

- **Supabase PostgreSQL**: User, Order, Payment databases
- **MongoDB Atlas**: Product, Notification databases  
- **Redis Cloud**: Shopping Cart cache
- **CloudAMQP**: RabbitMQ message queue

Connection strings đã được config trong `appsettings.Development.json`.

## Cách chạy

### Option 1: Visual Studio
1. Set `ECommerce.AppHost` làm startup project
2. Nhấn F5 hoặc click Run
3. Dashboard sẽ tự động mở tại `http://localhost:15000`

### Option 2: Command Line
```bash
cd src/ECommerce.AppHost
dotnet run
```

### Option 3: JetBrains Rider
1. Mở Run Configuration
2. Chọn `ECommerce.AppHost`
3. Click Run

## Dashboard

Khi chạy, Aspire Dashboard sẽ mở tại `http://localhost:15000` với các tab:

- **Resources**: Xem status của tất cả services
- **Console Logs**: Logs từ tất cả services
- **Structured Logs**: Filtered và searchable logs
- **Traces**: Distributed tracing
- **Metrics**: Performance metrics

## Services được orchestrate

1. **user-api** (Port 5010, 5011 gRPC)
2. **product-api** (Port 5020, 5021 gRPC)
3. **cart-api** (Port 5030)
4. **order-api** (Port 5040, 5041 gRPC)
5. **payment-api** (Port 5050)
6. **notification-api** (Port 5060)
7. **apigateway** (Port 5000)

## So sánh với Docker Compose

### Docker Compose (Trước đây)
```bash
docker-compose up
# Phải quản lý nhiều terminals
# Khó debug
# Phải rebuild containers khi thay đổi code
```

### Aspire (Bây giờ)
```bash
dotnet run --project src/ECommerce.AppHost
# Tất cả services chạy cùng lúc
# Dashboard tích hợp
# Hot reload tự động
# Service discovery tự động
```

## Configuration

### Development
Connection strings trong `appsettings.Development.json` trỏ đến managed services.

### Production
Để deploy production, sử dụng Docker Compose hoặc Kubernetes. Aspire có thể generate manifest:

```bash
dotnet run --project src/ECommerce.AppHost -- --publisher manifest --output-path ../aspire-manifest.json
```

## Troubleshooting

### Services không start
- Kiểm tra connection strings trong `appsettings.Development.json`
- Verify managed services (Supabase, MongoDB Atlas, Redis Cloud) đang hoạt động

### Port conflicts
- Thay đổi ports trong launchSettings.json của từng service

### Dashboard không mở
- Truy cập thủ công: `http://localhost:15000`
- Kiểm tra firewall settings

## Tài liệu thêm

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard)
