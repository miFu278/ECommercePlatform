# ECommerce Platform - Docker Start Script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ECommerce Platform - Docker Startup  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build và start services
Write-Host "Building and starting services..." -ForegroundColor Yellow
docker-compose up -d --build

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Services Started Successfully!       " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "  API Gateway:      http://localhost:5000" -ForegroundColor White
Write-Host "  User Service:     http://localhost:5010" -ForegroundColor White
Write-Host "  Product Service:  http://localhost:5020" -ForegroundColor White
Write-Host "  Cart Service:     http://localhost:5030" -ForegroundColor White
Write-Host "  Order Service:    http://localhost:5040" -ForegroundColor White
Write-Host "  Payment Service:  http://localhost:5050" -ForegroundColor White
Write-Host "  Notification:     http://localhost:5060" -ForegroundColor White
Write-Host ""
Write-Host "Commands:" -ForegroundColor Yellow
Write-Host "  View logs:  docker-compose logs -f" -ForegroundColor Gray
Write-Host "  Stop all:   docker-compose down" -ForegroundColor Gray
Write-Host "  Restart:    docker-compose restart" -ForegroundColor Gray
Write-Host ""
