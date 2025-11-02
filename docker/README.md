# Docker Infrastructure Setup

Docker Compose configuration để chạy tất cả databases và infrastructure services.

## Services

| Service | Port | Username | Password | Purpose |
|---------|------|----------|----------|---------|
| PostgreSQL | 5432 | postgres | postgres | User, Order, Payment Services |
| MongoDB | 27017 | admin | admin123 | Product, Notification Services |
| Redis | 6379 | - | redis123 | Shopping Cart, Caching |
| RabbitMQ | 5672, 15672 | guest | guest | Message Broker |

## Quick Start

### 1. Start all services
```bash
cd docker
docker-compose -f docker-compose.infrastructure.yml up -d
```

### 2. Check status
```bash
docker-compose -f docker-compose.infrastructure.yml ps
```

### 3. View logs
```bash
# All services
docker-compose -f docker-compose.infrastructure.yml logs -f

# Specific service
docker-compose -f docker-compose.infrastructure.yml logs -f postgres
```

### 4. Stop services
```bash
docker-compose -f docker-compose.infrastructure.yml down
```

### 5. Stop and remove volumes (⚠️ Deletes all data)
```bash
docker-compose -f docker-compose.infrastructure.yml down -v
```

## PostgreSQL

### Databases Created
- `UserDb` - User Service
- `OrderDb` - Order Service
- `PaymentDb` - Payment Service

### Connection Strings

**From Host Machine:**
```
Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=postgres
```

**From Docker Container:**
```
Host=postgres;Port=5432;Database=UserDb;Username=postgres;Password=postgres
```

### Connect with psql
```bash
docker exec -it ecommerce-postgres psql -U postgres -d UserDb
```

### Common Commands
```sql
-- List databases
\l

-- Connect to database
\c UserDb

-- List tables
\dt

-- Describe table
\d users
```

## MongoDB

### Databases
- `ProductDb` - Product Catalog Service
- `NotificationDb` - Notification Service

### Connection Strings

**From Host Machine:**
```
mongodb://admin:admin123@localhost:27017
```

**From Docker Container:**
```
mongodb://admin:admin123@mongodb:27017
```

### Connect with mongosh
```bash
docker exec -it ecommerce-mongodb mongosh -u admin -p admin123
```

### Common Commands
```javascript
// Show databases
show dbs

// Use database
use ProductDb

// Show collections
show collections

// Find documents
db.products.find()
```

## Redis

### Connection

**From Host Machine:**
```
localhost:6379
Password: redis123
```

**From Docker Container:**
```
redis:6379
Password: redis123
```

### Connect with redis-cli
```bash
docker exec -it ecommerce-redis redis-cli -a redis123
```

### Common Commands
```bash
# Test connection
PING

# Get all keys
KEYS *

# Get value
GET cart:user-123

# Set value
SET test "Hello Redis"

# Delete key
DEL test
```

## RabbitMQ

### Management UI
Open browser: http://localhost:15672
- Username: `guest`
- Password: `guest`

### Connection

**From Host Machine:**
```
amqp://guest:guest@localhost:5672
```

**From Docker Container:**
```
amqp://guest:guest@rabbitmq:5672
```

## Health Checks

All services have health checks configured:

```bash
# Check health status
docker-compose -f docker-compose.infrastructure.yml ps

# Healthy services show: Up (healthy)
```

## Volumes

Data is persisted in Docker volumes:

```bash
# List volumes
docker volume ls | grep ecommerce

# Inspect volume
docker volume inspect docker_postgres_data

# Remove all volumes (⚠️ Deletes all data)
docker volume rm docker_postgres_data docker_mongodb_data docker_redis_data docker_rabbitmq_data
```

## Troubleshooting

### Port already in use
```bash
# Check what's using the port
netstat -ano | findstr :5432

# Stop the process or change port in docker-compose.yml
```

### Container won't start
```bash
# View logs
docker logs ecommerce-postgres

# Remove container and try again
docker rm -f ecommerce-postgres
docker-compose -f docker-compose.infrastructure.yml up -d postgres
```

### Reset everything
```bash
# Stop and remove everything
docker-compose -f docker-compose.infrastructure.yml down -v

# Remove network
docker network rm docker_ecommerce-network

# Start fresh
docker-compose -f docker-compose.infrastructure.yml up -d
```

## Backup & Restore

### PostgreSQL Backup
```bash
# Backup single database
docker exec ecommerce-postgres pg_dump -U postgres UserDb > backup_userdb.sql

# Restore
docker exec -i ecommerce-postgres psql -U postgres UserDb < backup_userdb.sql
```

### MongoDB Backup
```bash
# Backup
docker exec ecommerce-mongodb mongodump --username admin --password admin123 --authenticationDatabase admin --out /backup

# Copy from container
docker cp ecommerce-mongodb:/backup ./mongodb_backup

# Restore
docker exec ecommerce-mongodb mongorestore --username admin --password admin123 --authenticationDatabase admin /backup
```

### Redis Backup
```bash
# Trigger save
docker exec ecommerce-redis redis-cli -a redis123 SAVE

# Copy RDB file
docker cp ecommerce-redis:/data/dump.rdb ./redis_backup.rdb
```

## Production Considerations

⚠️ **This setup is for DEVELOPMENT only!**

For production:
1. Use strong passwords
2. Enable SSL/TLS
3. Configure proper networking
4. Set up automated backups
5. Use managed database services (AWS RDS, Azure Database, etc.)
6. Implement monitoring and alerting

## Connection Strings for appsettings.json

```json
{
  "ConnectionStrings": {
    "UserDb": "Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=postgres",
    "OrderDb": "Host=localhost;Port=5432;Database=OrderDb;Username=postgres;Password=postgres",
    "PaymentDb": "Host=localhost;Port=5432;Database=PaymentDb;Username=postgres;Password=postgres",
    "ProductDb": "mongodb://admin:admin123@localhost:27017/ProductDb",
    "NotificationDb": "mongodb://admin:admin123@localhost:27017/NotificationDb",
    "Redis": "localhost:6379,password=redis123"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

---

**Status**: ✅ Ready to use  
**Last Updated**: November 2025
