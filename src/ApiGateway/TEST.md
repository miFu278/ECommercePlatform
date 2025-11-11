# Hướng dẫn Test API Gateway

## Bước 1: Chạy các services

Mở 3 terminal riêng biệt:

**Terminal 1 - User Service:**
```bash
cd src/Services/Users/ECommerce.User.API
dotnet run
```
Chạy trên: `http://localhost:5000`

**Terminal 2 - Product Service:**
```bash
cd src/Services/Product/ECommerce.Product.API
dotnet run
```
Chạy trên: `http://localhost:5010`

**Terminal 3 - API Gateway:**
```bash
cd src/ApiGateway
dotnet run
```
Chạy trên: `http://localhost:5050`

## Bước 2: Test routing

### Test User Service qua Gateway

```bash
# Trước đây (gọi trực tiếp):
curl http://localhost:5000/api/users

# Bây giờ (qua Gateway):
curl http://localhost:5050/users
```

### Test Product Service qua Gateway

```bash
# Trước đây (gọi trực tiếp):
curl http://localhost:5010/api/products

# Bây giờ (qua Gateway):
curl http://localhost:5050/products
```

## Bước 3: Test với Postman/Thunder Client

Import các request sau:

**GET Users:**
- URL: `http://localhost:5050/users`
- Method: GET

**GET Products:**
- URL: `http://localhost:5050/products`
- Method: GET

**POST User:**
- URL: `http://localhost:5050/users`
- Method: POST
- Body: (theo schema của User Service)

## Kiểm tra logs

Khi gọi API qua Gateway, bạn sẽ thấy logs ở cả 2 nơi:
1. Terminal API Gateway - nhận request từ client
2. Terminal User/Product Service - xử lý request thực tế

## Troubleshooting

**Lỗi: Connection refused**
- Đảm bảo User Service và Product Service đang chạy
- Kiểm tra ports trong `ocelot.Development.json` khớp với services

**Lỗi: 404 Not Found**
- Kiểm tra route trong `ocelot.Development.json`
- Đảm bảo `UpstreamPathTemplate` và `DownstreamPathTemplate` đúng

**Lỗi: Port already in use**
- Đổi port trong `launchSettings.json`
- Hoặc kill process đang dùng port đó
