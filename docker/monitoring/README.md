# API Gateway Monitoring với Prometheus + Grafana

## Tổng quan

Stack monitoring này bao gồm:
- **Prometheus**: Thu thập metrics từ API Gateway
- **Grafana**: Hiển thị dashboard trực quan

## Metrics được track

### HTTP Metrics (tự động)
- `http_requests_received_total` - Tổng số requests
- `http_request_duration_seconds` - Thời gian xử lý request
- `http_requests_in_progress` - Số requests đang xử lý

### System Metrics
- `process_cpu_seconds_total` - CPU usage
- `process_working_set_bytes` - Memory usage
- `dotnet_collection_count_total` - GC collections

## Cách chạy

### Bước 1: Start Prometheus + Grafana

```bash
cd docker
docker-compose -f docker-compose.monitoring.yml up -d
```

### Bước 2: Start API Gateway

```bash
cd src/ApiGateway
dotnet run
```

### Bước 3: Kiểm tra metrics endpoint

Mở browser: `http://localhost:5050/metrics`

Bạn sẽ thấy metrics dạng:
```
# HELP http_requests_received_total Total number of HTTP requests
# TYPE http_requests_received_total counter
http_requests_received_total{code="200",method="GET"} 42
```

### Bước 4: Truy cập Prometheus

URL: `http://localhost:9090`

Test query:
```
rate(http_requests_received_total[5m])
```

### Bước 5: Truy cập Grafana

URL: `http://localhost:3000`
- Username: `admin`
- Password: `admin`

Dashboard "API Gateway Monitoring" đã được tự động import.

## Test với traffic

Gọi API nhiều lần để tạo metrics:

```bash
# Windows PowerShell
for ($i=1; $i -le 100; $i++) { 
    Invoke-WebRequest -Uri http://localhost:5050/users -Method GET
    Start-Sleep -Milliseconds 100
}
```

Hoặc dùng tool như Apache Bench:
```bash
ab -n 1000 -c 10 http://localhost:5050/users
```

## Dashboard panels

1. **Total Requests** - Tổng requests/giây
2. **Request Rate by Route** - Biểu đồ requests theo route
3. **Response Time (p95)** - 95% requests hoàn thành trong bao lâu
4. **Error Rate** - Tỷ lệ lỗi 5xx

## Queries hữu ích

### Top slowest endpoints
```
topk(5, histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])))
```

### Error rate percentage
```
sum(rate(http_requests_received_total{code=~"5.."}[5m])) / sum(rate(http_requests_received_total[5m])) * 100
```

### Requests per minute
```
sum(rate(http_requests_received_total[1m])) * 60
```

## Tắt monitoring

```bash
cd docker
docker-compose -f docker-compose.monitoring.yml down
```

Giữ data:
```bash
docker-compose -f docker-compose.monitoring.yml down
```

Xóa hết data:
```bash
docker-compose -f docker-compose.monitoring.yml down -v
```

## Troubleshooting

**Prometheus không thu thập được metrics:**
- Kiểm tra API Gateway đang chạy: `http://localhost:5050/metrics`
- Kiểm tra Prometheus targets: `http://localhost:9090/targets`
- Đảm bảo `host.docker.internal` hoạt động (Docker Desktop)

**Grafana không hiển thị data:**
- Kiểm tra datasource đã connect: Settings → Data Sources
- Kiểm tra time range trong dashboard (mặc định: last 15 minutes)
- Đảm bảo có traffic đến API Gateway

**Port conflicts:**
- Prometheus: 9090
- Grafana: 3000
- Nếu bị conflict, đổi port trong `docker-compose.monitoring.yml`
