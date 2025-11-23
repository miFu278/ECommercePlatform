# Start Redis for Shopping Cart Service

Write-Host "🚀 Starting Redis..." -ForegroundColor Cyan

# Check if Redis container already exists
$existing = docker ps -a --filter "name=redis" --format "{{.Names}}"

if ($existing -eq "redis") {
    Write-Host "📦 Redis container already exists" -ForegroundColor Yellow
    
    # Check if it's running
    $running = docker ps --filter "name=redis" --format "{{.Names}}"
    
    if ($running -eq "redis") {
        Write-Host "✅ Redis is already running!" -ForegroundColor Green
    } else {
        Write-Host "▶️  Starting existing Redis container..." -ForegroundColor Cyan
        docker start redis
        Write-Host "✅ Redis started!" -ForegroundColor Green
    }
} else {
    Write-Host "📦 Creating new Redis container..." -ForegroundColor Cyan
    docker run -d --name redis -p 6379:6379 redis:7-alpine
    Write-Host "✅ Redis created and started!" -ForegroundColor Green
}

Write-Host ""
Write-Host "📊 Redis Info:" -ForegroundColor Cyan
Write-Host "   Host: localhost" -ForegroundColor White
Write-Host "   Port: 6379" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Test Redis:" -ForegroundColor Cyan
Write-Host "   docker exec -it redis redis-cli" -ForegroundColor White
Write-Host "   PING" -ForegroundColor White
Write-Host ""
Write-Host "🛑 Stop Redis:" -ForegroundColor Cyan
Write-Host "   docker stop redis" -ForegroundColor White
Write-Host ""
Write-Host "🗑️  Remove Redis:" -ForegroundColor Cyan
Write-Host "   docker rm -f redis" -ForegroundColor White
