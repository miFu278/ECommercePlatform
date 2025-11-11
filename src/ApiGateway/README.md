# API Gateway - Ocelot

API Gateway cho ECommerce Platform sử dụng Ocelot.

## Cổng (Ports)

- **API Gateway**: `http://localhost:5050`
- **User Service**: `http://localhost:5000`
- **Product Service**: `http://localhost:5010`

## Routing

| Client Request | Gateway Route | Backend Service |
|---------------|---------------|-----------------|
| `GET /users/*` | → | `http://localhost:5000/api/users/*` |
| `GET /products/*` | → | `http://localhost:5010/api/products/*` |

## Chạy API Gateway

```bash
cd src/ApiGateway
dotnet run
```

## Test

```bash
# Thay vì gọi trực tiếp:
# http://localhost:5000/api/users
# http://localhost:5010/api/products

# Giờ gọi qua Gateway:
curl http://localhost:5050/users
curl http://localhost:5050/products
```

## Cấu hình theo môi trường

- `ocelot.Development.json` - Localhost (dev)
- `ocelot.Production.json` - Production URLs (cần update khi deploy)

## Thêm route mới

Thêm vào `ocelot.Development.json`:

```json
{
  "DownstreamPathTemplate": "/api/orders/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5020
    }
  ],
  "UpstreamPathTemplate": "/orders/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
}
```

## Tính năng có thể thêm sau

- Authentication/Authorization (JWT validation)
- Rate Limiting
- Caching
- Load Balancing
- Service Discovery (Consul)
