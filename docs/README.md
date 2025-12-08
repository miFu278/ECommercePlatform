# Tài Liệu E-Commerce Platform

Chào mừng đến với tài liệu của E-Commerce Microservices Platform!

## 📚 Mục Lục

### 🎯 Bắt Đầu
- [README Chính](../README.md) - Tổng quan dự án và hướng dẫn bắt đầu nhanh
- [Báo Cáo Hoàn Thiện](../COMPLETENESS_REPORT.md) - Trạng thái hiện tại của dự án
- [Hướng Dẫn Đóng Góp](../CONTRIBUTING.md) - Cách đóng góp vào dự án

### 🏗️ Kiến Trúc & Thiết Kế
- [Kiến Trúc Hệ Thống](core/architecture.md) - Kiến trúc microservices, design patterns, tech stack
- [Thiết Kế Cơ Sở Dữ Liệu](core/database-document.md) - Database schemas, migrations, backup strategies
- [Cấu Trúc Dự Án](core/project-structure.md) - Tổ chức code, naming conventions, dependencies

### 🌐 API Documentation
- [Tài Liệu API](api/api-document.md) - API endpoints đầy đủ cho tất cả services
  - User Service API
  - Product Catalog API
  - Shopping Cart API
  - Order Service API
  - Payment Service API
  - Notification Service API
  - Error handling & Rate limiting

### 🚀 Deployment & Hosting
- [Hướng Dẫn Deployment](deployment/deployment.md) - Local, Docker, Kubernetes deployment
- [Tùy Chọn Hosting](deployment/hosting-options.md) - So sánh các nền tảng cloud hosting
- [Docker Single Server](deployment/docker-single-server.md) - Deploy trên single server với Docker
- [Render Deployment](deployment/render-deployment.md) - Deploy lên Render.com
- [Free Hosting Comparison](deployment/free-hosting-comparison.md) - So sánh các options hosting miễn phí

### 🛠️ Công Cụ & Cấu Hình
- [CodeRabbit Setup](tools/CODERABBIT_SETUP.md) - Cấu hình AI code review

## 📖 Hướng Dẫn Đọc Tài Liệu

### Nếu bạn là Developer mới
1. Đọc [README Chính](../README.md) để hiểu tổng quan
2. Xem [Cấu Trúc Dự Án](core/project-structure.md) để hiểu cách tổ chức code
3. Đọc [Kiến Trúc Hệ Thống](core/architecture.md) để hiểu design patterns
4. Tham khảo [Tài Liệu API](api/api-document.md) khi phát triển

### Nếu bạn muốn Deploy
1. Đọc [Hướng Dẫn Deployment](deployment/deployment.md) cho overview
2. Chọn platform phù hợp từ [Tùy Chọn Hosting](deployment/hosting-options.md)
3. Follow hướng dẫn cụ thể cho platform đã chọn

### Nếu bạn làm việc với Database
1. Xem [Thiết Kế Cơ Sở Dữ Liệu](core/database-document.md)
2. Hiểu database schema cho từng service
3. Tìm hiểu migration strategies

## 🎓 Learning Path

### Week 1: Fundamentals
- [ ] Đọc README và hiểu kiến trúc tổng thể
- [ ] Setup môi trường development local
- [ ] Chạy thử các services
- [ ] Test API endpoints với Swagger

### Week 2: Deep Dive
- [ ] Nghiên cứu Clean Architecture implementation
- [ ] Hiểu communication patterns (REST, gRPC, Events)
- [ ] Tìm hiểu database design cho từng service
- [ ] Đọc code của 1-2 services

### Week 3: Development
- [ ] Tạo feature mới trong 1 service
- [ ] Viết unit tests
- [ ] Implement inter-service communication
- [ ] Submit pull request

### Week 4: Deployment
- [ ] Deploy lên môi trường test
- [ ] Setup CI/CD pipeline
- [ ] Configure monitoring và logging
- [ ] Deploy lên production

## 📊 Sơ Đồ Kiến Trúc

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────┐
│                  API Gateway (Ocelot)                    │
│         JWT Auth | Rate Limiting | Load Balancing       │
└────────┬────────────────────────────────────────────────┘
         │
    ┌────┴────┬────────┬────────┬────────┬────────┐
    ↓         ↓        ↓        ↓        ↓        ↓
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│  User  │ │Product │ │  Cart  │ │ Order  │ │Payment │ │ Notify │
│Service │ │Service │ │Service │ │Service │ │Service │ │Service │
│        │ │        │ │        │ │        │ │        │ │        │
│  :5001 │ │  :5002 │ │  :5003 │ │  :5004 │ │  :5005 │ │  :5006 │
└───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘
    │          │          │          │          │          │
    ↓          ↓          ↓          ↓          ↓          ↓
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│Postgres│ │MongoDB │ │ Redis  │ │Postgres│ │Postgres│ │MongoDB │
│ UserDb │ │ProductDb│ │  Cart  │ │OrderDb │ │PaymentDb│ │NotifyDb│
└────────┘ └────────┘ └────────┘ └────────┘ └────────┘ └────────┘
         │
    ┌────┴────┬────────┐
    ↓         ↓        ↓
┌────────┐ ┌────────┐ ┌────────┐
│RabbitMQ│ │  Seq   │ │ Consul │
│ Events │ │  Logs  │ │Discovery│
└────────┘ └────────┘ └────────┘
```

### Service Communication
```
┌──────────────┐
│ User Service │
└──────┬───────┘
       │ gRPC
       ↓
┌──────────────┐      gRPC      ┌──────────────┐
│ Order Service│←────────────────│Payment Service│
└──────┬───────┘                 └──────────────┘
       │ gRPC
       ↓
┌──────────────┐      gRPC      ┌──────────────┐
│Product Service│←────────────────│  Cart Service│
└──────────────┘                 └──────────────┘
       │
       │ Events (RabbitMQ)
       ↓
┌──────────────┐
│Notification  │
│   Service    │
└──────────────┘
```

## 🔍 Quick Reference

### Service Ports
| Service | HTTP | HTTPS | gRPC |
|---------|------|-------|------|
| API Gateway | 5000 | 5443 | - |
| User Service | 5001 | 5444 | 5101 |
| Product Service | 5002 | 5445 | 5102 |
| Cart Service | 5003 | 5446 | 5103 |
| Order Service | 5004 | 5447 | 5104 |
| Payment Service | 5005 | 5448 | 5105 |
| Notification Service | 5006 | 5449 | 5106 |

### Infrastructure Ports
| Service | Port | Management UI |
|---------|------|---------------|
| PostgreSQL | 5432 | - |
| MongoDB | 27017 | - |
| Redis | 6379 | - |
| RabbitMQ | 5672 | 15672 |
| Seq | 5341 | - |
| Consul | 8500 | 8500 |

### Common Commands

**Start Infrastructure:**
```bash
cd docker
docker-compose up -d
```

**Run Service:**
```bash
cd src/Services/{ServiceName}/ECommerce.{ServiceName}.API
dotnet run
```

**Run Migrations:**
```bash
dotnet ef database update --project ../ECommerce.{ServiceName}.Infrastructure
```

**Build Solution:**
```bash
dotnet build
```

**Run Tests:**
```bash
dotnet test
```

## 🤝 Đóng Góp Tài Liệu

Nếu bạn tìm thấy lỗi hoặc muốn cải thiện tài liệu:

1. Fork repository
2. Tạo branch: `git checkout -b docs/improve-documentation`
3. Commit changes: `git commit -m "docs: improve API documentation"`
4. Push: `git push origin docs/improve-documentation`
5. Tạo Pull Request

## 📞 Hỗ Trợ

- **Issues**: [GitHub Issues](https://github.com/miFu278/ECommercePlatform/issues)
- **Discussions**: [GitHub Discussions](https://github.com/miFu278/ECommercePlatform/discussions)
- **Email**: phucttm.dev@gmail.com

## 📝 License

Tài liệu này được cấp phép theo giấy phép MIT - xem file [LICENSE](../LICENSE) để biết chi tiết.

---

**Cập Nhật Lần Cuối**: Tháng 12 năm 2025  
**Phiên Bản Tài Liệu**: 1.0  
**Người Duy Trì**: Development Team
