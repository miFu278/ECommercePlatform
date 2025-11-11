# Start Monitoring Stack (Prometheus + Grafana)

Write-Host "Starting Prometheus + Grafana..." -ForegroundColor Green

docker-compose -f docker-compose.monitoring.yml up -d

Write-Host ""
Write-Host "Monitoring stack started successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Yellow
Write-Host "  Prometheus: http://localhost:9090" -ForegroundColor Cyan
Write-Host "  Grafana:    http://localhost:3000 (admin/admin)" -ForegroundColor Cyan
Write-Host ""
Write-Host "API Gateway metrics: http://localhost:5050/metrics" -ForegroundColor Yellow
Write-Host ""
Write-Host "To stop: docker-compose -f docker-compose.monitoring.yml down" -ForegroundColor Gray
