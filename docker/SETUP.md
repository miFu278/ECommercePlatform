# Docker Setup Guide

## Prerequisites

### 1. Install Docker Desktop

**Windows:**
1. Download Docker Desktop from: https://www.docker.com/products/docker-desktop
2. Install and restart computer
3. Start Docker Desktop
4. Wait for Docker to be running (check system tray icon)

**Verify Installation:**
```powershell
docker --version
docker-compose --version
```

## Setup Steps

### Step 1: Start Docker Desktop
Make sure Docker Desktop is running (check system tray)

### Step 2: Navigate to docker folder
```powershell
cd docker
```

### Step 3: Start all services
```powershell
# Option 1: Using PowerShell script
.\start.ps1

# Option 2: Using docker-compose directly
docker-compose -f docker-compose.infrastructure.yml up -d
```

### Step 4: Verify services are running
```powershell
docker-compose -f docker-compose.infrastructure.yml ps
```

Expected output:
```
NAME                   STATUS              PORTS
ecommerce-postgres     Up (healthy)        0.0.0.0:5432->5432/tcp
ecommerce-mongodb      Up (healthy)        0.0.0.0:27017->27017/tcp
ecommerce-redis        Up (healthy)        0.0.0.0:6379->6379/tcp
ecommerce-rabbitmq     Up (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
```

### Step 5: Test connections

**PostgreSQL:**
```powershell
docker exec -it ecommerce-postgres psql -U postgres -c "\l"
```

**MongoDB:**
```powershell
docker exec -it ecommerce-mongodb mongosh -u admin -p admin123 --eval "show dbs"
```

**Redis:**
```powershell
docker exec -it ecommerce-redis redis-cli -a redis123 PING
```

**RabbitMQ:**
Open browser: http://localhost:15672
Login: guest / guest

## Common Issues

### Issue 1: Docker Desktop not running
**Error:** `error during connect: ... dockerDesktopLinuxEngine: The system cannot find the file specified`

**Solution:** Start Docker Desktop and wait for it to be ready

### Issue 2: Port already in use
**Error:** `Bind for 0.0.0.0:5432 failed: port is already allocated`

**Solution:**
```powershell
# Check what's using the port
netstat -ano | findstr :5432

# Stop the process or change port in docker-compose.yml
```

### Issue 3: Permission denied
**Error:** `permission denied while trying to connect to the Docker daemon socket`

**Solution:** Run PowerShell as Administrator

### Issue 4: Services not healthy
**Error:** Services show as "unhealthy"

**Solution:**
```powershell
# View logs
docker-compose -f docker-compose.infrastructure.yml logs postgres

# Restart service
docker-compose -f docker-compose.infrastructure.yml restart postgres
```

## Stop Services

```powershell
# Option 1: Using PowerShell script
.\stop.ps1

# Option 2: Using docker-compose
docker-compose -f docker-compose.infrastructure.yml down

# Option 3: Stop and remove all data
docker-compose -f docker-compose.infrastructure.yml down -v
```

## Next Steps

After infrastructure is running:

1. **Update appsettings.json** in your API projects with connection strings
2. **Run EF Core migrations** to create database schema
3. **Start your services**

### Example: Run User Service migrations
```powershell
cd ../src/Services/Users/ECommerce.User.API
dotnet ef database update --project ../ECommerce.User.Infrastructure
```

---

**Need Help?** Check the main README.md in the docker folder for detailed documentation.
