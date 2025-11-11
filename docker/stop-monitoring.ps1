# Stop Monitoring Stack

Write-Host "Stopping Prometheus + Grafana..." -ForegroundColor Yellow

docker-compose -f docker-compose.monitoring.yml down

Write-Host ""
Write-Host "Monitoring stack stopped!" -ForegroundColor Green
Write-Host ""
Write-Host "To remove all data: docker-compose -f docker-compose.monitoring.yml down -v" -ForegroundColor Gray
