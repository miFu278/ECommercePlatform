# Start all infrastructure services
Write-Host "🚀 Starting E-Commerce Infrastructure..." -ForegroundColor Green

docker-compose -f docker-compose.infrastructure.yml up -d

Write-Host ""
Write-Host "⏳ Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Cyan
docker-compose -f docker-compose.infrastructure.yml ps

Write-Host ""
Write-Host "✅ Infrastructure is ready!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Access Information:" -ForegroundColor Cyan
Write-Host "  PostgreSQL:  localhost:5432 (user: postgres, pass: postgres)" -ForegroundColor White
Write-Host "  MongoDB:     localhost:27017 (user: admin, pass: admin123)" -ForegroundColor White
Write-Host "  Redis:       localhost:6379 (pass: redis123)" -ForegroundColor White
Write-Host "  RabbitMQ:    localhost:5672" -ForegroundColor White
Write-Host "  RabbitMQ UI: http://localhost:15672 (user: guest, pass: guest)" -ForegroundColor White
Write-Host ""
Write-Host "📖 View logs: docker-compose -f docker-compose.infrastructure.yml logs -f" -ForegroundColor Yellow
Write-Host "🛑 Stop all:  docker-compose -f docker-compose.infrastructure.yml down" -ForegroundColor Yellow
