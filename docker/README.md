# Docker Setup - ECommerce Platform

## Quick Start

```powershell
cd docker
.\start.ps1
```

Hoặc:
```powershell
cd docker
docker-compose up -d --build
```

## Services

| Service | Port | gRPC Port | URL |
|---------|------|-----------|-----|
| API Gateway | 5000 | - | http://localhost:5000 |
| User Service | 5010 | 5011 | http://localhost:5010 |
| Product Service | 5020 | 5021 | http://localhost:5020 |
| Cart Service | 5030 | - | http://localhost:5030 |
| Order Service | 5040 | 5041 | http://localhost:5040 |
| Payment Service | 5050 | - | http://localhost:5050 |
| Notification Service | 5060 | - | http://localhost:5060 |

## Cloud Databases

Databases đã được setup trên cloud:
- **PostgreSQL**: Supabase
- **MongoDB**: Atlas
- **Redis**: Redis Cloud

Config trong `appsettings.Development.json` của mỗi service.

## Commands

```powershell
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f user-service

# Stop all services
docker-compose down

# Restart a service
docker-compose restart user-service

# Rebuild a service
docker-compose up -d --build user-service
```

## Monitoring (Optional)

```powershell
docker-compose -f docker-compose.monitoring.yml up -d
```

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
