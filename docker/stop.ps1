# Stop all infrastructure services
Write-Host "🛑 Stopping E-Commerce Infrastructure..." -ForegroundColor Yellow

docker-compose -f docker-compose.infrastructure.yml down

Write-Host ""
Write-Host "✅ All services stopped!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 To remove all data, run:" -ForegroundColor Cyan
Write-Host "   docker-compose -f docker-compose.infrastructure.yml down -v" -ForegroundColor White
