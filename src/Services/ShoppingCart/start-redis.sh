#!/bin/bash
# Start Redis for Shopping Cart Service

echo "🚀 Starting Redis..."

# Check if Redis container already exists
if [ "$(docker ps -a -q -f name=redis)" ]; then
    echo "📦 Redis container already exists"
    
    # Check if it's running
    if [ "$(docker ps -q -f name=redis)" ]; then
        echo "✅ Redis is already running!"
    else
        echo "▶️  Starting existing Redis container..."
        docker start redis
        echo "✅ Redis started!"
    fi
else
    echo "📦 Creating new Redis container..."
    docker run -d --name redis -p 6379:6379 redis:7-alpine
    echo "✅ Redis created and started!"
fi

echo ""
echo "📊 Redis Info:"
echo "   Host: localhost"
echo "   Port: 6379"
echo ""
echo "🧪 Test Redis:"
echo "   docker exec -it redis redis-cli"
echo "   PING"
echo ""
echo "🛑 Stop Redis:"
echo "   docker stop redis"
echo ""
echo "🗑️  Remove Redis:"
echo "   docker rm -f redis"
