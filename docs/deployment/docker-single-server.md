# Deploy Tất Cả Services với Docker Compose (1 Server)

## 🎯 Tổng quan

Thay vì deploy 5-6 services riêng lẻ, bạn có thể:
- ✅ Deploy tất cả services cùng lúc trên 1 server
- ✅ Dùng Docker Compose để quản lý
- ✅ Connect đến cloud databases (đã có sẵn)
- ✅ Chỉ cần 1 VPS/server

## 💰 Chi phí

### Free Options:
1. **Oracle Cloud** - FREE FOREVER
   - 2 VMs miễn phí (1GB RAM, 1 CPU)
   - 200GB storage
   - Link: https://www.oracle.com/cloud/free/

2. **Google Cloud** - $300 credit (90 ngày)
   - Sau đó ~$5-10/tháng cho 1 VM nhỏ
   - Link: https://cloud.google.com/free

3. **Azure** - $200 credit (30 ngày)
   - Link: https://azure.microsoft.com/free

### Paid Options (Rẻ):
1. **DigitalOcean** - $4/tháng
   - 1GB RAM, 1 CPU, 25GB SSD
   - Link: https://www.digitalocean.com

2. **Vultr** - $2.50/tháng
   - 512MB RAM, 1 CPU, 10GB SSD
   - Link: https://www.vultr.com

3. **Contabo** - €4.50/tháng (~$5)
   - 4GB RAM, 2 CPU, 50GB SSD
   - Link: https://contabo.com

---

## 📋 Bước 1: Chuẩn bị

### 1.1. Tạo file .env

Copy từ `.env.example` và điền thông tin:

```bash
cd ECommercePlatform
cp .env.example .env
```

Sửa file `.env`:

```bash
# PostgreSQL (Supabase, Render, Railway, etc.)
USER_DB_CONNECTION_STRING=Host=db.xxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-pass

# MongoDB Atlas
MONGODB_CONNECTION_STRING=mongodb+srv://user:pass@cluster0.xxxxx.mongodb.net/ProductDb?retryWrites=true&w=majority

# Upstash Redis
REDIS_CONNECTION_STRING=redis://default:your-pass@global-xxx.upstash.io:6379

# JWT Secret (tạo random string dài 32+ ký tự)
JWT_SECRET=abc123xyz789-change-this-to-random-string-min-32-chars
```

### 1.2. Test local trước

```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# Check logs
docker-compose logs -f

# Test API
curl http://localhost:5000/health  # User Service
curl http://localhost:5001/health  # Product Service
curl http://localhost:5002/health  # Cart Service

# Stop
docker-compose down
```

---

## 📋 Bước 2: Chọn Server

### Option A: Oracle Cloud (FREE FOREVER - Khuyến nghị)

#### 1. Đăng ký Oracle Cloud
1. Vào: https://www.oracle.com/cloud/free/
2. Sign up (cần credit card để verify, nhưng KHÔNG tính phí)
3. Chọn region gần VN: Singapore, Tokyo, Seoul

#### 2. Tạo VM Instance
1. Menu → Compute → Instances → **Create Instance**
2. Chọn:
   - **Name**: ecommerce-server
   - **Image**: Ubuntu 22.04
   - **Shape**: VM.Standard.E2.1.Micro (FREE tier)
   - **RAM**: 1GB
   - **Storage**: 50GB
3. **Add SSH Keys**: Upload public key hoặc generate mới
4. Click **Create**

#### 3. Mở Ports
1. Instance Details → **Subnet** → **Security List**
2. Add Ingress Rules:
   ```
   Source: 0.0.0.0/0
   Port: 80 (HTTP)
   Port: 443 (HTTPS)
   Port: 5000-5005 (Services)
   ```

#### 4. Connect SSH
```bash
ssh ubuntu@<PUBLIC_IP>
```

---

### Option B: DigitalOcean ($4/tháng)

#### 1. Tạo Droplet
1. Vào: https://www.digitalocean.com
2. Create → Droplets
3. Chọn:
   - **Image**: Ubuntu 22.04
   - **Plan**: Basic ($4/month)
   - **Region**: Singapore
4. Add SSH key
5. Create Droplet

#### 2. Connect
```bash
ssh root@<DROPLET_IP>
```

---

## 📋 Bước 3: Setup Server

### 3.1. Install Docker

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo apt install docker-compose -y

# Verify
docker --version
docker-compose --version

# Add user to docker group (optional)
sudo usermod -aG docker $USER
```

### 3.2. Install Git

```bash
sudo apt install git -y
```

---

## 📋 Bước 4: Deploy

### 4.1. Clone Repository

```bash
# Clone repo
git clone https://github.com/your-username/ECommercePlatform.git
cd ECommercePlatform
```

### 4.2. Create .env file

```bash
# Tạo file .env
nano .env
```

Paste nội dung (thay thông tin thật):

```bash
USER_DB_CONNECTION_STRING=Host=your-db.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=xxx
MONGODB_CONNECTION_STRING=mongodb+srv://user:pass@cluster.mongodb.net/ProductDb
REDIS_CONNECTION_STRING=redis://default:pass@redis.upstash.io:6379
JWT_SECRET=your-random-secret-key-min-32-characters-long
```

Save: `Ctrl+X` → `Y` → `Enter`

### 4.3. Build và Start

```bash
# Build images (lần đầu mất ~10-15 phút)
docker-compose build

# Start services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

### 4.4. Verify

```bash
# Test từ server
curl http://localhost:5000/health

# Test từ máy local
curl http://<SERVER_IP>:5000/health
```

---

## 📋 Bước 5: Setup Nginx (Optional - Recommended)

Để có domain đẹp và HTTPS:

### 5.1. Install Nginx

```bash
sudo apt install nginx -y
```

### 5.2. Configure Nginx

```bash
sudo nano /etc/nginx/sites-available/ecommerce
```

Paste:

```nginx
# User Service
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Product Service
server {
    listen 80;
    server_name product.yourdomain.com;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}

# Cart Service
server {
    listen 80;
    server_name cart.yourdomain.com;

    location / {
        proxy_pass http://localhost:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable:

```bash
sudo ln -s /etc/nginx/sites-available/ecommerce /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 5.3. Setup SSL (Free HTTPS)

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Get SSL certificate
sudo certbot --nginx -d api.yourdomain.com -d product.yourdomain.com -d cart.yourdomain.com

# Auto-renew
sudo certbot renew --dry-run
```

---

## 🔧 Quản lý Services

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f user-service

# Last 100 lines
docker-compose logs --tail=100 user-service
```

### Restart Services

```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart user-service
```

### Update Code

```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose up -d --build

# Or rebuild specific service
docker-compose up -d --build user-service
```

### Stop Services

```bash
# Stop all
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Check Resource Usage

```bash
# Docker stats
docker stats

# Disk usage
docker system df

# Clean up
docker system prune -a
```

---

## 📊 Monitoring

### Simple Health Check Script

```bash
# Tạo file check-health.sh
nano check-health.sh
```

```bash
#!/bin/bash

services=(
  "User:5000"
  "Product:5001"
  "Cart:5002"
  "Order:5003"
  "Payment:5004"
  "Notification:5005"
)

for service in "${services[@]}"; do
  name="${service%%:*}"
  port="${service##*:}"
  
  status=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$port/health)
  
  if [ "$status" = "200" ]; then
    echo "✅ $name Service: OK"
  else
    echo "❌ $name Service: FAILED (HTTP $status)"
  fi
done
```

```bash
chmod +x check-health.sh
./check-health.sh
```

### Setup Cron Job

```bash
# Auto check every 5 minutes
crontab -e
```

Add:

```bash
*/5 * * * * /home/ubuntu/ECommercePlatform/check-health.sh >> /var/log/health-check.log 2>&1
```

---

## 🔐 Security Checklist

- [ ] Đổi JWT_SECRET thành random string mạnh
- [ ] Không commit .env vào Git
- [ ] Setup firewall (UFW)
  ```bash
  sudo ufw allow 22/tcp
  sudo ufw allow 80/tcp
  sudo ufw allow 443/tcp
  sudo ufw enable
  ```
- [ ] Disable root SSH login
- [ ] Setup fail2ban
- [ ] Regular updates
  ```bash
  sudo apt update && sudo apt upgrade -y
  ```
- [ ] Backup databases regularly

---

## 💡 Pro Tips

### 1. Auto-restart on server reboot

```bash
# Services sẽ tự động start khi server reboot
# (đã có restart: unless-stopped trong docker-compose.yml)
```

### 2. Limit Docker logs size

```bash
# Tạo /etc/docker/daemon.json
sudo nano /etc/docker/daemon.json
```

```json
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
```

```bash
sudo systemctl restart docker
```

### 3. Setup swap (nếu RAM ít)

```bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

---

## 🎯 Tổng kết

**Ưu điểm cách này:**
- ✅ Deploy 1 lần, chạy tất cả services
- ✅ Dễ quản lý với docker-compose
- ✅ Chi phí thấp (có thể FREE với Oracle)
- ✅ Không phụ thuộc nhiều platforms

**Nhược điểm:**
- ❌ Tất cả services trên 1 server (single point of failure)
- ❌ Khó scale từng service riêng
- ❌ Cần quản lý server (updates, security)

**Phù hợp cho:**
- MVP, side projects
- Budget thấp
- Team nhỏ
- Traffic vừa phải (<1000 users)

---

## 📞 Troubleshooting

### Services không start

```bash
# Check logs
docker-compose logs user-service

# Check if port is used
sudo netstat -tulpn | grep 5000

# Restart
docker-compose restart user-service
```

### Out of memory

```bash
# Check memory
free -h

# Add swap (xem Pro Tips)
# Hoặc upgrade server
```

### Database connection failed

```bash
# Test connection từ server
# PostgreSQL
psql "Host=xxx;Database=xxx;Username=xxx;Password=xxx"

# MongoDB
mongosh "mongodb+srv://xxx"

# Redis
redis-cli -h xxx -p 6379 -a password ping
```

---

**Chúc bạn deploy thành công! 🚀**
