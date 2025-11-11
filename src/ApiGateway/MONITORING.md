# Quick Start - API Gateway Monitoring

## 1. Start Monitoring Stack

```powershell
cd docker
.\start-monitoring.ps1
```

Hoặc manual:
```bash
docker-compose -f docker-compose.monitoring.yml up -d
```

## 2. Start API Gateway

```bash
cd src/ApiGateway
dotnet run
```

## 3. Verify Metrics

Mở browser: http://localhost:5050/metrics

Bạn sẽ thấy output như:
```
# HELP http_requests_received_total Total number of HTTP requests
# TYPE http_requests_received_total counter
http_requests_received_total{code="200",method="GET",controller="",action=""} 0

# HELP http_request_duration_seconds HTTP request duration
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{le="0.005"} 0
```

## 4. Generate Traffic

```powershell
# Gọi API 100 lần
for ($i=1; $i -le 100; $i++) { 
    Invoke-WebRequest -Uri http://localhost:5050/users -Method GET
    Start-Sleep -Milliseconds 100
}
```

## 5. View Dashboard

### Prometheus
- URL: http://localhost:9090
- Query example: `rate(http_requests_received_total[5m])`

### Grafana
- URL: http://localhost:3000
- Login: admin / admin
- Dashboard: "API Gateway Monitoring"

## Dashboard Preview

Bạn sẽ thấy:

📊 **Total Requests** - Real-time request rate
```
Current: 15.2 req/s
```

📈 **Request Rate by Route** - Line chart
```
/users:    10 req/s
/products: 5 req/s
```

⏱️ **Response Time (p95)** - Latency
```
p95: 45ms
p99: 120ms
```

❌ **Error Rate** - 5xx errors
```
Error rate: 0.5%
```

## Custom Metrics (Optional)

Nếu muốn thêm custom metrics trong API Gateway:

```csharp
using Prometheus;

// Counter
private static readonly Counter RequestCounter = Metrics
    .CreateCounter("gateway_requests_total", "Total gateway requests");

// Histogram
private static readonly Histogram RequestDuration = Metrics
    .CreateHistogram("gateway_request_duration", "Gateway request duration");

// Usage
RequestCounter.Inc();
using (RequestDuration.NewTimer())
{
    // Your code
}
```

## Stop Monitoring

```powershell
cd docker
.\stop-monitoring.ps1
```

## Troubleshooting

**Metrics endpoint returns 404:**
- Đảm bảo đã cài package: `prometheus-net.AspNetCore`
- Kiểm tra `app.UseMetricServer()` trong Program.cs

**Grafana không có data:**
- Kiểm tra Prometheus targets: http://localhost:9090/targets
- Target phải là "UP" (màu xanh)
- Đảm bảo có traffic đến API Gateway

**Docker không start:**
- Kiểm tra port 9090 và 3000 có bị chiếm không
- Chạy: `docker ps` để xem containers đang chạy
