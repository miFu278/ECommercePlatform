# E-Commerce Platform - Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Docker Setup](#docker-setup)
4. [Kubernetes Deployment](#kubernetes-deployment)
5. [CI/CD Pipeline](#cicd-pipeline)
6. [Environment Configuration](#environment-configuration)
7. [Database Migration](#database-migration)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Security Configuration](#security-configuration)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Minimum Version | Purpose |
|----------|----------------|---------|
| .NET SDK | 8.0 | Application runtime |
| Docker | 24.0+ | Containerization |
| Docker Compose | 2.20+ | Multi-container orchestration |
| Kubernetes (kubectl) | 1.28+ | Production orchestration |
| Helm | 3.12+ | Kubernetes package manager |
| Git | 2.40+ | Version control |
| PostgreSQL | 15+ | Relational database |
| MongoDB | 6.0+ | Document database |
| Redis | 7.0+ | Caching and session storage |
| RabbitMQ | 3.12+ | Message broker |

### Recommended Tools
- Visual Studio 2022 / VS Code
- Postman or Insomnia (API testing)
- Azure Data Studio / pgAdmin (Database management)
- K9s or Lens (Kubernetes management)
- Portainer (Docker management)

### System Requirements

**Development Machine:**
- OS: Windows 10/11, macOS 12+, or Linux
- RAM: 16GB minimum, 32GB recommended
- CPU: 4 cores minimum, 8 cores recommended
- Disk: 50GB free space

**Production Server:**
- RAM: 32GB minimum per node
- CPU: 8 cores minimum per node
- Disk: 500GB SSD minimum
- Network: 1Gbps minimum

---

## Local Development Setup

### Step 1: Clone Repository

```bash
git clone https://github.com/your-org/ecommerce-platform.git
cd ecommerce-platform
```

### Step 2: Install .NET Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Install development certificates
dotnet dev-certs https --trust
```

### Step 3: Setup Local Databases

#### Option A: Using Docker Compose (Recommended)

```bash
# Start infrastructure services only
docker-compose -f docker-compose.infrastructure.yml up -d
```

#### Option B: Manual Installation

**PostgreSQL:**
```bash
# Ubuntu/Debian
sudo apt-get install postgresql-15

# macOS
brew install postgresql@15

# Windows: Download installer from postgresql.org
```

**MongoDB:**
```bash
# Ubuntu/Debian
sudo apt-get install mongodb-org

# macOS
brew install mongodb-community@6.0

# Windows: Download installer from mongodb.com
```

**Redis:**
```bash
# Ubuntu/Debian
sudo apt-get install redis-server

# macOS
brew install redis

# Windows: Download from redis.io
```

**RabbitMQ:**
```bash
# Ubuntu/Debian
sudo apt-get install rabbitmq-server

# macOS
brew install rabbitmq

# Windows: Download installer from rabbitmq.com
```

### Step 4: Configure Environment Variables

Create `.env` file in the root directory:

```env
# Database Connections
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password_here
POSTGRES_HOST=localhost
POSTGRES_PORT=5432

MONGODB_HOST=localhost
MONGODB_PORT=27017

REDIS_HOST=localhost
REDIS_PORT=6379

RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# JWT Settings
JWT_SECRET=your_super_secret_jwt_key_here_min_32_chars
JWT_ISSUER=ECommerceAPI
JWT_AUDIENCE=ECommerceClient
JWT_EXPIRATION_MINUTES=60

# IdentityServer
IDENTITY_SERVER_URL=https://localhost:5001

# External Services
STRIPE_SECRET_KEY=sk_test_your_stripe_key
STRIPE_PUBLISHABLE_KEY=pk_test_your_stripe_key

SENDGRID_API_KEY=your_sendgrid_api_key
TWILIO_ACCOUNT_SID=your_twilio_sid
TWILIO_AUTH_TOKEN=your_twilio_token

# Logging
SEQ_URL=http://localhost:5341
SEQ_API_KEY=your_seq_api_key

# Service Discovery
CONSUL_URL=http://localhost:8500

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### Step 5: Run Database Migrations

```bash
# User Service
cd src/Services/User/ECommerce.User.API
dotnet ef database update

# Order Service
cd ../../../Order/ECommerce.Order.API
dotnet ef database update

# Payment Service
cd ../../../Payment/ECommerce.Payment.API
dotnet ef database update
```

### Step 6: Run Services Locally

**Terminal 1 - API Gateway:**
```bash
cd src/ApiGateway/ECommerce.Gateway
dotnet run
```

**Terminal 2 - User Service:**
```bash
cd src/Services/User/ECommerce.User.API
dotnet run
```

**Terminal 3 - Product Service:**
```bash
cd src/Services/Product/ECommerce.Product.API
dotnet run
```

**Terminal 4 - Shopping Cart Service:**
```bash
cd src/Services/ShoppingCart/ECommerce.ShoppingCart.API
dotnet run
```

**Terminal 5 - Order Service:**
```bash
cd src/Services/Order/ECommerce.Order.API
dotnet run
```

**Terminal 6 - Payment Service:**
```bash
cd src/Services/Payment/ECommerce.Payment.API
dotnet run
```

**Terminal 7 - Notification Service:**
```bash
cd src/Services/Notification/ECommerce.Notification.API
dotnet run
```

### Step 7: Verify Services

Check service health:
```bash
# API Gateway
curl http://localhost:5000/health

# Individual services
curl http://localhost:5001/health  # User Service
curl http://localhost:5002/health  # Product Service
curl http://localhost:5003/health  # Shopping Cart Service
curl http://localhost:5004/health  # Order Service
curl http://localhost:5005/health  # Payment Service
curl http://localhost:5006/health  # Notification Service
```

---

## Docker Setup

### Project Structure with Docker

```
ECommercePlatform/
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   ├── docker-compose.infrastructure.yml
│   └── .env
├── src/
│   ├── ApiGateway/
│   │   └── ECommerce.Gateway/
│   │       └── Dockerfile
│   └── Services/
│       ├── User/
│       │   └── ECommerce.User.API/
│       │       └── Dockerfile
│       └── ...
```

### Dockerfile Example

**src/Services/User/ECommerce.User.API/Dockerfile:**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Services/User/ECommerce.User.API/ECommerce.User.API.csproj", "Services/User/ECommerce.User.API/"]
COPY ["Services/User/ECommerce.User.Application/ECommerce.User.Application.csproj", "Services/User/ECommerce.User.Application/"]
COPY ["Services/User/ECommerce.User.Domain/ECommerce.User.Domain.csproj", "Services/User/ECommerce.User.Domain/"]
COPY ["Services/User/ECommerce.User.Infrastructure/ECommerce.User.Infrastructure.csproj", "Services/User/ECommerce.User.Infrastructure/"]
COPY ["BuildingBlocks/ECommerce.Common/ECommerce.Common.csproj", "BuildingBlocks/ECommerce.Common/"]

RUN dotnet restore "Services/User/ECommerce.User.API/ECommerce.User.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Services/User/ECommerce.User.API"
RUN dotnet build "ECommerce.User.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.User.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.User.API.dll"]
```

### Docker Compose - Infrastructure Services

**docker/docker-compose.infrastructure.yml:**
```yaml
version: '3.8'

services:
  # PostgreSQL
  postgres:
    image: postgres:15-alpine
    container_name: ecommerce-postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      POSTGRES_MULTIPLE_DATABASES: UserDb,OrderDb,PaymentDb
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init-scripts/postgres:/docker-entrypoint-initdb.d
    networks:
      - ecommerce-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # MongoDB
  mongodb:
    image: mongo:6
    container_name: ecommerce-mongodb
    environment:
      MONGO_INITDB_ROOT_USERNAME: ${MONGODB_USER:-admin}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGODB_PASSWORD:-admin}
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
      - mongodb_config:/data/configdb
    networks:
      - ecommerce-network
    healthcheck:
      test: echo 'db.runCommand("ping").ok' | mongosh localhost:27017/test --quiet
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis
  redis:
    image: redis:7-alpine
    container_name: ecommerce-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - ecommerce-network
    command: redis-server --appendonly yes
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: ecommerce-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-guest}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:-guest}
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - ecommerce-network
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 10s
      timeout: 5s
      retries: 5

  # Consul
  consul:
    image: consul:latest
    container_name: ecommerce-consul
    ports:
      - "8500:8500"
      - "8600:8600/udp"
    volumes:
      - consul_data:/consul/data
    networks:
      - ecommerce-network
    command: agent -server -ui -bootstrap-expect=1 -client=0.0.0.0

  # Seq (Logging)
  seq:
    image: datalust/seq:latest
    container_name: ecommerce-seq
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    networks:
      - ecommerce-network

  # Jaeger (Distributed Tracing)
  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: ecommerce-jaeger
    ports:
      - "5775:5775/udp"
      - "6831:6831/udp"
      - "6832:6832/udp"
      - "5778:5778"
      - "16686:16686"
      - "14268:14268"
      - "14250:14250"
      - "9411:9411"
    networks:
      - ecommerce-network

  # Prometheus
  prometheus:
    image: prom/prometheus:latest
    container_name: ecommerce-prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    networks:
      - ecommerce-network
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'

  # Grafana
  grafana:
    image: grafana/grafana:latest
    container_name: ecommerce-grafana
    ports:
      - "3000:3000"
    environment:
      GF_SECURITY_ADMIN_USER: admin
      GF_SECURITY_ADMIN_PASSWORD: admin
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./grafana/datasources:/etc/grafana/provisioning/datasources
    networks:
      - ecommerce-network
    depends_on:
      - prometheus

volumes:
  postgres_data:
  mongodb_data:
  mongodb_config:
  redis_data:
  rabbitmq_data:
  consul_data:
  seq_data:
  prometheus_data:
  grafana_data:

networks:
  ecommerce-network:
    driver: bridge
```

### Docker Compose - Application Services

**docker/docker-compose.yml:**
```yaml
version: '3.8'

services:
  # API Gateway
  api-gateway:
    build:
      context: ../
      dockerfile: src/ApiGateway/ECommerce.Gateway/Dockerfile
    container_name: ecommerce-api-gateway
    ports:
      - "5000:80"
      - "5443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
    depends_on:
      - consul
      - seq
      - user-service
      - product-service
      - shoppingcart-service
      - order-service
      - payment-service
      - notification-service
    networks:
      - ecommerce-network
    restart: on-failure

  # User Service
  user-service:
    build:
      context: ../
      dockerfile: src/Services/User/ECommerce.User.API/Dockerfile
    container_name: ecommerce-user-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=UserDb;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - JWT__Secret=${JWT_SECRET}
      - JWT__Issuer=${JWT_ISSUER}
      - JWT__Audience=${JWT_AUDIENCE}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Product Service
  product-service:
    build:
      context: ../
      dockerfile: src/Services/Product/ECommerce.Product.API/Dockerfile
    container_name: ecommerce-product-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__MongoDB=mongodb://${MONGODB_USER}:${MONGODB_PASSWORD}@mongodb:27017
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      mongodb:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Shopping Cart Service
  shoppingcart-service:
    build:
      context: ../
      dockerfile: src/Services/ShoppingCart/ECommerce.ShoppingCart.API/Dockerfile
    container_name: ecommerce-shoppingcart-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Order Service
  order-service:
    build:
      context: ../
      dockerfile: src/Services/Order/ECommerce.Order.API/Dockerfile
    container_name: ecommerce-order-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=OrderDb;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Payment Service
  payment-service:
    build:
      context: ../
      dockerfile: src/Services/Payment/ECommerce.Payment.API/Dockerfile
    container_name: ecommerce-payment-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=PaymentDb;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
      - Stripe__PublishableKey=${STRIPE_PUBLISHABLE_KEY}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Notification Service
  notification-service:
    build:
      context: ../
      dockerfile: src/Services/Notification/ECommerce.Notification.API/Dockerfile
    container_name: ecommerce-notification-service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__MongoDB=mongodb://${MONGODB_USER}:${MONGODB_PASSWORD}@mongodb:27017
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - SendGrid__ApiKey=${SENDGRID_API_KEY}
      - Twilio__AccountSid=${TWILIO_ACCOUNT_SID}
      - Twilio__AuthToken=${TWILIO_AUTH_TOKEN}
      - Consul__Host=consul
      - Consul__Port=8500
      - Logging__Seq__ServerUrl=http://seq:5341
      - Jaeger__AgentHost=jaeger
      - Jaeger__AgentPort=6831
    depends_on:
      mongodb:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
      seq:
        condition: service_started
    networks:
      - ecommerce-network
    restart: on-failure
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

networks:
  ecommerce-network:
    external: true
```

### Running with Docker Compose

```bash
# Start infrastructure services
cd docker
docker-compose -f docker-compose.infrastructure.yml up -d

# Wait for services to be healthy
docker-compose -f docker-compose.infrastructure.yml ps

# Create network
docker network create ecommerce-network

# Build and start application services
docker-compose up --build -d

# View logs
docker-compose logs -f

# Check service status
docker-compose ps

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Docker Commands Cheat Sheet

```bash
# Build specific service
docker-compose build user-service

# Restart specific service
docker-compose restart user-service

# View service logs
docker-compose logs -f user-service

# Execute command in container
docker-compose exec user-service bash

# Scale service
docker-compose up -d --scale product-service=3

# Remove stopped containers
docker-compose rm

# Remove everything including volumes
docker-compose down -v --remove-orphans
```

---

## Kubernetes Deployment

### Prerequisites

```bash
# Install kubectl
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl

# Install Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Verify installation
kubectl version --client
helm version
```

### Cluster Setup

#### Option A: Local Kubernetes (Minikube/Kind)

```bash
# Install minikube
curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube

# Start cluster
minikube start --cpus=4 --memory=8192 --driver=docker

# Enable ingress
minikube addons enable ingress
minikube addons enable metrics-server
```

#### Option B: Cloud Kubernetes (AKS/EKS/GKE)

**Azure Kubernetes Service (AKS):**
```bash
# Create resource group
az group create --name ecommerce-rg --location eastus

# Create AKS cluster
az aks create \
  --resource-group ecommerce-rg \
  --name ecommerce-cluster \
  --node-count 3 \
  --node-vm-size Standard_D4s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group ecommerce-rg --name ecommerce-cluster
```

### Kubernetes Manifests

**k8s/namespace.yaml:**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: ecommerce
  labels:
    name: ecommerce
```

**k8s/configmap.yaml:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: ecommerce-config
  namespace: ecommerce
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Consul__Host: "consul"
  Consul__Port: "8500"
  RabbitMQ__Host: "rabbitmq"
  RabbitMQ__Port: "5672"
  Logging__Seq__ServerUrl: "http://seq:5341"
```

**k8s/secrets.yaml:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ecommerce-secrets
  namespace: ecommerce
type: Opaque
stringData:
  POSTGRES_PASSWORD: "your_postgres_password"
  MONGODB_PASSWORD: "your_mongodb_password"
  JWT_SECRET: "your_jwt_secret_key_min_32_chars"
  STRIPE_SECRET_KEY: "sk_live_your_stripe_key"
  SENDGRID_API_KEY: "your_sendgrid_key"
```

**k8s/deployments/user-service.yaml:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
  namespace: ecommerce
  labels:
    app: user-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: user-service
  template:
    metadata:
      labels:
        app: user-service
    spec:
      containers:
      - name: user-service
        image: your-registry/ecommerce-user-service:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 80
          name: http
        - containerPort: 443
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: ecommerce-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Port=5432;Database=UserDb;Username=postgres;Password=$(POSTGRES_PASSWORD)"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: ecommerce-secrets
              key: POSTGRES_PASSWORD
        - name: JWT__Secret
          valueFrom:
            secretKeyRef:
              name: ecommerce-secrets
              key: JWT_SECRET
        envFrom:
        - configMapRef:
            name: ecommerce-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "200m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: ecommerce
  labels:
    app: user-service
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 80
    protocol: TCP
    name: http
  selector:
    app: user-service
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
  namespace: ecommerce
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

**k8s/ingress.yaml:**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ecommerce-ingress
  namespace: ecommerce
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/rate-limit: "100"
spec:
  tls:
  - hosts:
    - api.ecommerce.com
    secretName: ecommerce-tls
  rules:
  - host: api.ecommerce.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-gateway
            port:
              number: 80
```

### Deploy to Kubernetes

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create ConfigMap and Secrets
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml

# Deploy infrastructure services
kubectl apply -f k8s/infrastructure/

# Deploy application services
kubectl apply -f k8s/deployments/

# Deploy services
kubectl apply -f k8s/services/

# Deploy ingress
kubectl apply -f k8s/ingress.yaml

# Verify deployments
kubectl get all -n ecommerce

# Check pod status
kubectl get pods -n ecommerce

# View logs
kubectl logs -f deployment/user-service -n ecommerce

# Describe pod for troubleshooting
kubectl describe pod <pod-name> -n ecommerce
```

### Helm Chart Deployment

**helm/ecommerce-platform/Chart.yaml:**
```yaml
apiVersion: v2
name: ecommerce-platform
description: E-Commerce Microservices Platform
type: application
version: 1.0.0
appVersion: "1.0.0"
```

**helm/ecommerce-platform/values.yaml:**
```yaml
global:
  environment: production
  imageRegistry: your-registry.azurecr.io
  imagePullPolicy: Always

apiGateway:
  enabled: true
  replicaCount: 3
  image:
    repository: ecommerce/api-gateway
    tag: latest
  service:
    type: LoadBalancer
    port: 80

userService:
  enabled: true
  replicaCount: 3
  image:
    repository: ecommerce/user-service
    tag: latest
  database:
    type: postgresql
    name: UserDb

productService:
  enabled: true
  replicaCount: 3
  image:
    repository: ecommerce/product-service
    tag: latest
  database:
    type: mongodb
    name: ProductDb

shoppingCartService:
  enabled: true
  replicaCount: 2
  image:
    repository: ecommerce/shoppingcart-service
    tag: latest
  database:
    type: redis

orderService:
  enabled: true
  replicaCount: 3
  image:
    repository: ecommerce/order-service
    tag: latest
  database:
    type: postgresql
    name: OrderDb

paymentService:
  enabled: true
  replicaCount: 2
  image:
    repository: ecommerce/payment-service
    tag: latest
  database:
    type: postgresql
    name: PaymentDb

notificationService:
  enabled: true
  replicaCount: 2
  image:
    repository: ecommerce/notification-service
    tag: latest
  database:
    type: mongodb
    name: NotificationDb

postgresql:
  enabled: true
  auth:
    username: postgres
    password: postgres
    database: postgres
  primary:
    persistence:
      size: 50Gi

mongodb:
  enabled: true
  auth:
    rootPassword: mongodb
  persistence:
    size: 50Gi

redis:
  enabled: true
  auth:
    enabled: false
  master:
    persistence:
      size: 10Gi

rabbitmq:
  enabled: true
  auth:
    username: guest
    password: guest
  persistence:
    size: 20Gi

ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
  hosts:
    - host: api.ecommerce.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: ecommerce-tls
      hosts:
        - api.ecommerce.com

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

monitoring:
  enabled: true
  prometheus:
    enabled: true
  grafana:
    enabled: true
```

**Deploy with Helm:**
```bash
# Add required Helm repositories
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# Install the chart
helm install ecommerce-platform ./helm/ecommerce-platform \
  --namespace ecommerce \
  --create-namespace \
  --values ./helm/ecommerce-platform/values.yaml

# Upgrade existing deployment
helm upgrade ecommerce-platform ./helm/ecommerce-platform \
  --namespace ecommerce \
  --values ./helm/ecommerce-platform/values.yaml

# Check deployment status
helm status ecommerce-platform -n ecommerce

# Rollback to previous version
helm rollback ecommerce-platform -n ecommerce

# Uninstall
helm uninstall ecommerce-platform -n ecommerce
```

---

## CI/CD Pipeline

### GitHub Actions Workflow

**.github/workflows/ci-cd.yml:**
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: 
          - user-service
          - product-service
          - shoppingcart-service
          - order-service
          - payment-service
          - notification-service
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Run unit tests
      run: dotnet test tests/ECommerce.${{ matrix.service }}.Tests --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"

    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
        flags: ${{ matrix.service }}

  build-docker-images:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    strategy:
      matrix:
        service:
          - api-gateway
          - user-service
          - product-service
          - shoppingcart-service
          - order-service
          - payment-service
          - notification-service

    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2

    - name: Log in to Container Registry
      uses: docker/login-action@v2
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v4
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/${{ matrix.service }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=semver,pattern={{version}}
          type=semver,pattern={{major}}.{{minor}}
          type=sha,prefix={{branch}}-

    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        file: src/Services/${{ matrix.service }}/Dockerfile
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy-to-staging:
    needs: build-docker-images
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment:
      name: staging
      url: https://staging.ecommerce.com

    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Configure kubectl
      uses: azure/k8s-set-context@v3
      with:
        method: kubeconfig
        kubeconfig: ${{ secrets.KUBE_CONFIG_STAGING }}

    - name: Deploy to Staging
      run: |
        helm upgrade --install ecommerce-platform ./helm/ecommerce-platform \
          --namespace staging \
          --create-namespace \
          --values ./helm/ecommerce-platform/values-staging.yaml \
          --set global.imageRegistry=${{ env.REGISTRY }} \
          --set global.imageTag=${{ github.sha }}

    - name: Run smoke tests
      run: |
        kubectl wait --for=condition=ready pod -l app=api-gateway -n staging --timeout=300s
        curl -f https://staging.ecommerce.com/health || exit 1

  deploy-to-production:
    needs: deploy-to-staging
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment:
      name: production
      url: https://api.ecommerce.com

    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Configure kubectl
      uses: azure/k8s-set-context@v3
      with:
        method: kubeconfig
        kubeconfig: ${{ secrets.KUBE_CONFIG_PROD }}

    - name: Deploy to Production
      run: |
        helm upgrade --install ecommerce-platform ./helm/ecommerce-platform \
          --namespace production \
          --create-namespace \
          --values ./helm/ecommerce-platform/values-production.yaml \
          --set global.imageRegistry=${{ env.REGISTRY }} \
          --set global.imageTag=${{ github.sha }}

    - name: Run smoke tests
      run: |
        kubectl wait --for=condition=ready pod -l app=api-gateway -n production --timeout=300s
        curl -f https://api.ecommerce.com/health || exit 1

    - name: Notify Slack
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        text: 'Deployment to production completed!'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
      if: always()
```

### Azure DevOps Pipeline

**azure-pipelines.yml:**
```yaml
trigger:
  branches:
    include:
    - main
    - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dockerRegistryServiceConnection: 'ecommerce-acr'
  containerRegistry: 'ecommerce.azurecr.io'
  kubernetesServiceConnection: 'ecommerce-aks'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - job: BuildAndTest
    displayName: 'Build and Run Tests'
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '8.0.x'

    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'

    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore'

    - task: DotNetCoreCLI@2
      displayName: 'Run unit tests'
      inputs:
        command: 'test'
        projects: 'tests/**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build --collect:"XPlat Code Coverage"'

    - task: PublishCodeCoverageResults@1
      displayName: 'Publish code coverage'
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'

- stage: Docker
  displayName: 'Build Docker Images'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - job: BuildDockerImages
    displayName: 'Build and Push Docker Images'
    steps:
    - task: Docker@2
      displayName: 'Build User Service'
      inputs:
        command: buildAndPush
        repository: 'ecommerce/user-service'
        dockerfile: 'src/Services/User/ECommerce.User.API/Dockerfile'
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(Build.BuildId)
          latest

    - task: Docker@2
      displayName: 'Build Product Service'
      inputs:
        command: buildAndPush
        repository: 'ecommerce/product-service'
        dockerfile: 'src/Services/Product/ECommerce.Product.API/Dockerfile'
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(Build.BuildId)
          latest

- stage: Deploy
  displayName: 'Deploy to Kubernetes'
  dependsOn: Docker
  jobs:
  - deployment: DeployToAKS
    displayName: 'Deploy to AKS'
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: HelmDeploy@0
            displayName: 'Helm upgrade'
            inputs:
              connectionType: 'Kubernetes Service Connection'
              kubernetesServiceConnection: $(kubernetesServiceConnection)
              namespace: 'production'
              command: 'upgrade'
              chartType: 'FilePath'
              chartPath: 'helm/ecommerce-platform'
              releaseName: 'ecommerce-platform'
              overrideValues: |
                global.imageRegistry=$(containerRegistry)
                global.imageTag=$(Build.BuildId)
              waitForExecution: true
```

---

## Environment Configuration

### Development Environment

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=UserDb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "JWT": {
    "Secret": "your_development_jwt_secret_key_min_32_chars",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceClient",
    "ExpirationMinutes": 60
  },
  "Consul": {
    "Host": "localhost",
    "Port": 8500
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341",
    "ApiKey": ""
  },
  "Jaeger": {
    "AgentHost": "localhost",
    "AgentPort": 6831
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200"
    ]
  }
}
```

### Staging Environment

**appsettings.Staging.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres-staging;Port=5432;Database=UserDb;Username=postgres;Password=${POSTGRES_PASSWORD}",
    "Redis": "redis-staging:6379"
  },
  "RabbitMQ": {
    "Host": "rabbitmq-staging",
    "Port": 5672,
    "Username": "${RABBITMQ_USERNAME}",
    "Password": "${RABBITMQ_PASSWORD}"
  },
  "JWT": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "ECommerceAPI",
    "Audience": "ECommerceClient",
    "ExpirationMinutes": 60
  },
  "Stripe": {
    "SecretKey": "${STRIPE_SECRET_KEY}",
    "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}",
    "WebhookSecret": "${STRIPE_WEBHOOK_SECRET}"
  }
}
```

### Production Environment

**appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=${POSTGRES_HOST};Port=5432;Database=UserDb;Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};SSL Mode=Require",
    "Redis": "${REDIS_HOST}:6379,password=${REDIS_PASSWORD},ssl=true"
  },
  "RabbitMQ": {
    "Host": "${RABBITMQ_HOST}",
    "Port": 5672,
    "Username": "${RABBITMQ_USERNAME}",
    "Password": "${RABBITMQ_PASSWORD}",
    "UseSsl": true
  },
  "Cors": {
    "AllowedOrigins": [
      "https://www.ecommerce.com",
      "https://ecommerce.com"
    ]
  }
}
```

---

## Database Migration

### Entity Framework Core Migrations

**Create Migration:**
```bash
cd src/Services/User/ECommerce.User.API

# Create new migration
dotnet ef migrations add InitialCreate --project ../ECommerce.User.Infrastructure

# Generate SQL script
dotnet ef migrations script --output migration.sql --project ../ECommerce.User.Infrastructure

# Apply migration
dotnet ef database update --project ../ECommerce.User.Infrastructure
```

**Migration in Docker:**
```dockerfile
# Add migration step to Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ... (build steps)

# Apply migrations on startup
FROM build AS migrate
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update"]
```

**Migration Job in Kubernetes:**
```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: user-service-migration
  namespace: ecommerce
spec:
  template:
    spec:
      containers:
      - name: migration
        image: your-registry/ecommerce-user-service:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Database=UserDb;Username=postgres;Password=$(POSTGRES_PASSWORD)"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: ecommerce-secrets
              key: POSTGRES_PASSWORD
      restartPolicy: OnFailure
  backoffLimit: 3
```

### MongoDB Migrations

**MongoDB.Migrations Package:**
```csharp
public class InitialProductSeed : Migration
{
    public InitialProductSeed() : base("1.0.0") { }

    public override void Up()
    {
        Database.GetCollection<Product>("products")
            .InsertMany(new[]
            {
                new Product 
                { 
                    Name = "Sample Product",
                    Price = 99.99m,
                    Stock = 100
                }
            });
    }

    public override void Down()
    {
        Database.GetCollection<Product>("products")
            .DeleteMany(Builders<Product>.Filter.Empty);
    }
}
```

**Apply MongoDB Migrations:**
```csharp
// Program.cs
var runner = new MigrationRunner("mongodb://localhost:27017", "ProductDb");
runner.MigrateUp();
```

---

## Monitoring and Logging

### Prometheus Configuration

**docker/prometheus/prometheus.yml:**
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'user-service'
    static_configs:
      - targets: ['user-service:80']
    metrics_path: '/metrics'

  - job_name: 'product-service'
    static_configs:
      - targets: ['product-service:80']

  - job_name: 'order-service'
    static_configs:
      - targets: ['order-service:80']

  - job_name: 'payment-service'
    static_configs:
      - targets: ['payment-service:80']

  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres-exporter:9187']

  - job_name: 'mongodb'
    static_configs:
      - targets: ['mongodb-exporter:9216']

  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']

  - job_name: 'rabbitmq'
    static_configs:
      - targets: ['rabbitmq:15692']
```

### Grafana Dashboards

**docker/grafana/dashboards/service-dashboard.json:**
```json
{
  "dashboard": {
    "title": "E-Commerce Services Dashboard",
    "panels": [
      {
        "title": "Request Rate",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])"
          }
        ]
      },
      {
        "title": "Response Time",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))"
          }
        ]
      },
      {
        "title": "Error Rate",
        "targets": [
          {
            "expr": "rate(http_requests_total{status=~\"5..\"}[5m])"
          }
        ]
      }
    ]
  }
}
```

### Serilog Configuration

**Program.cs:**
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "UserService")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"])
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Health Checks

**Program.cs:**
```csharp
builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        name: "postgres",
        tags: new[] { "db", "ready" })
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        tags: new[] { "cache", "ready" })
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"],
        name: "rabbitmq",
        tags: new[] { "messaging", "ready" });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

---

## Security Configuration

### HTTPS Configuration

**Program.cs:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Enable HTTPS in production
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});
```

### CORS Configuration

**Program.cs:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");
```

### Rate Limiting

**Program.cs:**
```csharp
using AspNetCoreRateLimit;

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

app.UseIpRateLimiting();
```

**appsettings.json:**
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*/api/*/auth/*",
        "Period": "1h",
        "Limit": 10
      }
    ]
  }
}
```

### Secrets Management

**Azure Key Vault:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

**AWS Secrets Manager:**
```csharp
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = entry => entry.Name.StartsWith("ecommerce/");
    options.PollingInterval = TimeSpan.FromHours(1);
});
```

---

## Troubleshooting

### Common Issues

#### 1. Container Won't Start

**Check logs:**
```bash
docker logs <container-name>
kubectl logs <pod-name> -n ecommerce
```

**Common causes:**
- Missing environment variables
- Database connection issues
- Port conflicts

#### 2. Database Connection Failed

**Test connection:**
```bash
# PostgreSQL
docker exec -it ecommerce-postgres psql -U postgres -c "SELECT 1"

# MongoDB
docker exec -it ecommerce-mongodb mongosh --eval "db.adminCommand('ping')"

# Redis
docker exec -it ecommerce-redis redis-cli ping
```

#### 3. Service Discovery Issues

**Check Consul:**
```bash
# View registered services
curl http://localhost:8500/v1/catalog/services

# Check service health
curl http://localhost:8500/v1/health/service/user-service
```

#### 4. Message Broker Problems

**Check RabbitMQ:**
```bash
# Management UI
http://localhost:15672

# Check queues
docker exec ecommerce-rabbitmq rabbitmqctl list_queues
```

#### 5. High Memory Usage

**Check memory:**
```bash
# Docker
docker stats

# Kubernetes
kubectl top pods -n ecommerce
kubectl top nodes
```

**Optimization:**
```csharp
// Reduce memory footprint
builder.Services.Configure<GCSettings>(options =>
{
    options.LatencyLevel = GCLatencyMode.Batch;
    options.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
});
```

### Debugging Commands

```bash
# View all resources
kubectl get all -n ecommerce

# Describe resource
kubectl describe pod <pod-name> -n ecommerce

# Get events
kubectl get events -n ecommerce --sort-by='.lastTimestamp'

# Execute command in pod
kubectl exec -it <pod-name> -n ecommerce -- /bin/bash

# Port forward for debugging
kubectl port-forward svc/user-service 8080:80 -n ecommerce

# View resource usage
kubectl top pods -n ecommerce
kubectl top nodes

# Check ingress
kubectl describe ingress ecommerce-ingress -n ecommerce

# View secrets (base64 decoded)
kubectl get secret ecommerce-secrets -n ecommerce -o jsonpath='{.data.JWT_SECRET}' | base64 --decode
```

### Performance Tuning

**Database Connection Pooling:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MaxBatchSize(100);
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(3);
    });
}, ServiceLifetime.Scoped, ServiceLifetime.Singleton);
```

**HTTP Client Configuration:**
```csharp
builder.Services.AddHttpClient<IProductService, ProductService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

**Redis Caching Strategy:**
```csharp
public class CacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, 
        DistributedCacheEntryOptions options = null)
    {
        var cached = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<T>(cached);
        }

        var value = await factory();
        var serialized = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serialized, options ?? _defaultOptions);
        
        return value;
    }
}
```

---

## Backup and Disaster Recovery

### Database Backup Strategy

**PostgreSQL Backup Script:**
```bash
#!/bin/bash
# backup-postgres.sh

BACKUP_DIR="/backups/postgres"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DATABASES=("UserDb" "OrderDb" "PaymentDb")

mkdir -p $BACKUP_DIR

for DB in "${DATABASES[@]}"; do
    echo "Backing up $DB..."
    docker exec ecommerce-postgres pg_dump -U postgres $DB | gzip > "$BACKUP_DIR/${DB}_${TIMESTAMP}.sql.gz"
    
    # Keep only last 7 days of backups
    find $BACKUP_DIR -name "${DB}_*.sql.gz" -mtime +7 -delete
done

echo "Backup completed at $(date)"
```

**MongoDB Backup Script:**
```bash
#!/bin/bash
# backup-mongodb.sh

BACKUP_DIR="/backups/mongodb"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

echo "Backing up MongoDB..."
docker exec ecommerce-mongodb mongodump --archive="/tmp/mongodb_${TIMESTAMP}.archive" --gzip

docker cp ecommerce-mongodb:/tmp/mongodb_${TIMESTAMP}.archive $BACKUP_DIR/

# Keep only last 7 days
find $BACKUP_DIR -name "mongodb_*.archive" -mtime +7 -delete

echo "MongoDB backup completed at $(date)"
```

**Automated Backup with Cron:**
```bash
# Add to crontab
crontab -e

# Backup databases daily at 2 AM
0 2 * * * /path/to/backup-postgres.sh >> /var/log/backup-postgres.log 2>&1
0 3 * * * /path/to/backup-mongodb.sh >> /var/log/backup-mongodb.log 2>&1
```

**Kubernetes CronJob for Backups:**
```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: postgres-backup
  namespace: ecommerce
spec:
  schedule: "0 2 * * *"
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: postgres:15
            command:
            - /bin/sh
            - -c
            - |
              pg_dump -h postgres -U postgres UserDb | gzip > /backup/userdb_$(date +%Y%m%d).sql.gz
              pg_dump -h postgres -U postgres OrderDb | gzip > /backup/orderdb_$(date +%Y%m%d).sql.gz
              pg_dump -h postgres -U postgres PaymentDb | gzip > /backup/paymentdb_$(date +%Y%m%d).sql.gz
            env:
            - name: PGPASSWORD
              valueFrom:
                secretKeyRef:
                  name: ecommerce-secrets
                  key: POSTGRES_PASSWORD
            volumeMounts:
            - name: backup
              mountPath: /backup
          volumes:
          - name: backup
            persistentVolumeClaim:
              claimName: backup-pvc
          restartPolicy: OnFailure
```

### Restore Procedures

**Restore PostgreSQL:**
```bash
# Restore from backup
gunzip -c /backups/postgres/UserDb_20251102_020000.sql.gz | \
  docker exec -i ecommerce-postgres psql -U postgres UserDb

# Or in Kubernetes
gunzip -c backup.sql.gz | kubectl exec -i postgres-0 -n ecommerce -- psql -U postgres UserDb
```

**Restore MongoDB:**
```bash
# Restore from archive
docker cp /backups/mongodb/mongodb_20251102_030000.archive ecommerce-mongodb:/tmp/
docker exec ecommerce-mongodb mongorestore --archive=/tmp/mongodb_20251102_030000.archive --gzip

# Or in Kubernetes
kubectl cp backup.archive ecommerce/mongodb-0:/tmp/backup.archive
kubectl exec -it mongodb-0 -n ecommerce -- mongorestore --archive=/tmp/backup.archive --gzip
```

### High Availability Setup

**PostgreSQL Replication:**
```yaml
# StatefulSet with replicas
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: ecommerce
spec:
  serviceName: postgres
  replicas: 3
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:15
        env:
        - name: POSTGRES_REPLICATION_MODE
          value: "master"
        - name: POSTGRES_REPLICATION_USER
          value: "replicator"
        - name: POSTGRES_REPLICATION_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgres-replication-secret
              key: password
        volumeMounts:
        - name: data
          mountPath: /var/lib/postgresql/data
  volumeClaimTemplates:
  - metadata:
      name: data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 50Gi
```

---

## Scaling Strategies

### Horizontal Pod Autoscaling

**CPU-based Autoscaling:**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
  namespace: ecommerce
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
      - type: Percent
        value: 100
        periodSeconds: 30
      - type: Pods
        value: 2
        periodSeconds: 30
      selectPolicy: Max
```

**Custom Metrics Autoscaling:**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-service-hpa
  namespace: ecommerce
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-service
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: "1000"
  - type: External
    external:
      metric:
        name: rabbitmq_queue_messages_ready
        selector:
          matchLabels:
            queue: order-processing
      target:
        type: AverageValue
        averageValue: "100"
```

### Vertical Pod Autoscaling

**VPA Configuration:**
```yaml
apiVersion: autoscaling.k8s.io/v1
kind: VerticalPodAutoscaler
metadata:
  name: user-service-vpa
  namespace: ecommerce
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  updatePolicy:
    updateMode: "Auto"
  resourcePolicy:
    containerPolicies:
    - containerName: user-service
      minAllowed:
        cpu: 100m
        memory: 128Mi
      maxAllowed:
        cpu: 2
        memory: 2Gi
      controlledResources: ["cpu", "memory"]
```

### Database Scaling

**Read Replicas:**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres-read
  namespace: ecommerce
spec:
  selector:
    app: postgres
    role: replica
  ports:
  - port: 5432
    targetPort: 5432
  type: ClusterIP
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres-replica
  namespace: ecommerce
spec:
  replicas: 2
  selector:
    matchLabels:
      app: postgres
      role: replica
  template:
    metadata:
      labels:
        app: postgres
        role: replica
    spec:
      containers:
      - name: postgres
        image: postgres:15
        env:
        - name: POSTGRES_REPLICATION_MODE
          value: "slave"
        - name: POSTGRES_MASTER_SERVICE
          value: "postgres-master"
```

**Connection Pooling with PgBouncer:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pgbouncer
  namespace: ecommerce
spec:
  replicas: 3
  selector:
    matchLabels:
      app: pgbouncer
  template:
    metadata:
      labels:
        app: pgbouncer
    spec:
      containers:
      - name: pgbouncer
        image: edoburu/pgbouncer:latest
        env:
        - name: DATABASE_URL
          value: "postgres://postgres:password@postgres:5432/UserDb"
        - name: POOL_MODE
          value: "transaction"
        - name: MAX_CLIENT_CONN
          value: "1000"
        - name: DEFAULT_POOL_SIZE
          value: "25"
        ports:
        - containerPort: 5432
```

---

## Monitoring Alerts

### Prometheus Alert Rules

**k8s/monitoring/alert-rules.yaml:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-alerts
  namespace: monitoring
data:
  alerts.yml: |
    groups:
    - name: ecommerce_alerts
      interval: 30s
      rules:
      # High Error Rate
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate on {{ $labels.service }}"
          description: "Error rate is {{ $value }} errors per second"

      # High Response Time
      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time on {{ $labels.service }}"
          description: "95th percentile response time is {{ $value }} seconds"

      # Service Down
      - alert: ServiceDown
        expr: up{job=~".*-service"} == 0
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Service {{ $labels.job }} is down"
          description: "Service has been down for more than 2 minutes"

      # High CPU Usage
      - alert: HighCPUUsage
        expr: rate(container_cpu_usage_seconds_total[5m]) > 0.8
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High CPU usage on {{ $labels.pod }}"
          description: "CPU usage is {{ $value }}%"

      # High Memory Usage
      - alert: HighMemoryUsage
        expr: container_memory_usage_bytes / container_spec_memory_limit_bytes > 0.9
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High memory usage on {{ $labels.pod }}"
          description: "Memory usage is {{ $value }}%"

      # Database Connection Pool Exhausted
      - alert: DatabaseConnectionPoolExhausted
        expr: db_connection_pool_active / db_connection_pool_size > 0.9
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Database connection pool nearly exhausted"
          description: "{{ $value }}% of connections are active"

      # RabbitMQ Queue Growing
      - alert: RabbitMQQueueGrowing
        expr: rabbitmq_queue_messages_ready > 1000
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "RabbitMQ queue {{ $labels.queue }} is growing"
          description: "Queue has {{ $value }} messages"

      # Disk Space Low
      - alert: DiskSpaceLow
        expr: (node_filesystem_avail_bytes / node_filesystem_size_bytes) * 100 < 10
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Disk space low on {{ $labels.instance }}"
          description: "Only {{ $value }}% disk space remaining"

      # Pod Restart Frequency
      - alert: PodRestartingFrequently
        expr: rate(kube_pod_container_status_restarts_total[1h]) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Pod {{ $labels.pod }} is restarting frequently"
          description: "Pod has restarted {{ $value }} times in the last hour"
```

### AlertManager Configuration

**k8s/monitoring/alertmanager-config.yaml:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: alertmanager-config
  namespace: monitoring
data:
  alertmanager.yml: |
    global:
      resolve_timeout: 5m
      slack_api_url: 'https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK'

    route:
      group_by: ['alertname', 'cluster', 'service']
      group_wait: 10s
      group_interval: 10s
      repeat_interval: 12h
      receiver: 'default'
      routes:
      - match:
          severity: critical
        receiver: 'critical'
        continue: true
      - match:
          severity: warning
        receiver: 'warning'

    receivers:
    - name: 'default'
      slack_configs:
      - channel: '#alerts'
        title: 'E-Commerce Alert'
        text: '{{ range .Alerts }}{{ .Annotations.summary }}\n{{ .Annotations.description }}\n{{ end }}'

    - name: 'critical'
      slack_configs:
      - channel: '#critical-alerts'
        title: 'CRITICAL: E-Commerce Alert'
        text: '{{ range .Alerts }}{{ .Annotations.summary }}\n{{ .Annotations.description }}\n{{ end }}'
      pagerduty_configs:
      - service_key: 'YOUR_PAGERDUTY_KEY'
      email_configs:
      - to: 'oncall@ecommerce.com'
        from: 'alerts@ecommerce.com'
        smarthost: 'smtp.gmail.com:587'
        auth_username: 'alerts@ecommerce.com'
        auth_password: 'your_password'

    - name: 'warning'
      slack_configs:
      - channel: '#warnings'
        title: 'Warning: E-Commerce Alert'
        text: '{{ range .Alerts }}{{ .Annotations.summary }}\n{{ .Annotations.description }}\n{{ end }}'

    inhibit_rules:
    - source_match:
        severity: 'critical'
      target_match:
        severity: 'warning'
      equal: ['alertname', 'cluster', 'service']
```

---

## Production Checklist

### Pre-Deployment Checklist

- [ ] All environment variables configured
- [ ] Secrets properly stored in Key Vault/Secrets Manager
- [ ] Database migrations tested
- [ ] SSL/TLS certificates configured
- [ ] Backup strategy implemented and tested
- [ ] Monitoring and alerting configured
- [ ] Load testing completed
- [ ] Security scan passed
- [ ] API documentation updated
- [ ] Runbook documentation complete
- [ ] Disaster recovery plan documented
- [ ] Health checks configured on all services
- [ ] Rate limiting configured
- [ ] CORS properly configured
- [ ] Logging centralized
- [ ] CI/CD pipeline tested

### Post-Deployment Verification

```bash
#!/bin/bash
# verify-deployment.sh

echo "Verifying E-Commerce Platform Deployment..."

# Check all pods are running
echo "Checking pod status..."
kubectl get pods -n ecommerce | grep -v Running && echo "ERROR: Some pods are not running" || echo "✓ All pods running"

# Check services
echo "Checking services..."
kubectl get svc -n ecommerce

# Health check
echo "Checking health endpoints..."
for service in api-gateway user-service product-service order-service payment-service; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://$service/health)
    if [ $STATUS -eq 200 ]; then
        echo "✓ $service is healthy"
    else
        echo "✗ $service returned status $STATUS"
    fi
done

# Check database connections
echo "Checking database connections..."
kubectl exec -it $(kubectl get pod -n ecommerce -l app=user-service -o jsonpath='{.items[0].metadata.name}') -n ecommerce -- curl localhost/health

# Check message broker
echo "Checking RabbitMQ..."
curl -u guest:guest http://localhost:15672/api/overview

# Check metrics endpoint
echo "Checking Prometheus targets..."
curl -s http://localhost:9090/api/v1/targets | jq '.data.activeTargets[] | {job: .labels.job, health: .health}'

echo "Deployment verification complete!"
```

### Performance Benchmarking

```bash
#!/bin/bash
# benchmark.sh

echo "Running performance benchmarks..."

# Install k6 if not present
which k6 || brew install k6

# Run load test
k6 run --vus 100 --duration 5m performance-tests/load-test.js

# Generate report
k6 run --out json=test-results.json performance-tests/load-test.js
```

**performance-tests/load-test.js:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200
    { duration: '5m', target: 200 }, // Stay at 200
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests under 500ms
    http_req_failed: ['rate<0.01'],   // Less than 1% errors
  },
};

const BASE_URL = 'https://api.ecommerce.com';

export default function () {
  // Get products
  let res = http.get(`${BASE_URL}/api/v1/products`);
  check(res, {
    'products status is 200': (r) => r.status === 200,
    'products response time OK': (r) => r.timings.duration < 500,
  });

  sleep(1);

  // Login
  res = http.post(`${BASE_URL}/api/v1/auth/login`, JSON.stringify({
    email: 'test@example.com',
    password: 'Password123!'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  
  check(res, {
    'login status is 200': (r) => r.status === 200,
  });

  const token = res.json('data.accessToken');

  sleep(1);

  // Add to cart
  res = http.post(`${BASE_URL}/api/v1/cart/items`, JSON.stringify({
    productId: 'prod-001',
    quantity: 1
  }), {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
  });

  check(res, {
    'add to cart status is 201': (r) => r.status === 201,
  });

  sleep(2);
}
```

---

## Maintenance Procedures

### Rolling Updates

```bash
# Update service image
kubectl set image deployment/user-service user-service=your-registry/user-service:v2 -n ecommerce

# Monitor rollout
kubectl rollout status deployment/user-service -n ecommerce

# Rollback if needed
kubectl rollout undo deployment/user-service -n ecommerce

# Rollback to specific revision
kubectl rollout undo deployment/user-service --to-revision=2 -n ecommerce

# View rollout history
kubectl rollout history deployment/user-service -n ecommerce
```

### Database Maintenance

**Vacuum PostgreSQL:**
```bash
#!/bin/bash
# vacuum-databases.sh

DATABASES=("UserDb" "OrderDb" "PaymentDb")

for DB in "${DATABASES[@]}"; do
    echo "Vacuuming $DB..."
    docker exec ecommerce-postgres psql -U postgres -d $DB -c "VACUUM ANALYZE;"
done
```

**MongoDB Maintenance:**
```bash
#!/bin/bash
# mongodb-maintenance.sh

echo "Running MongoDB maintenance..."

# Compact collections
docker exec ecommerce-mongodb mongosh --eval "
  db.getSiblingDB('ProductDb').getCollectionNames().forEach(function(collection) {
    print('Compacting ' + collection);
    db.getSiblingDB('ProductDb').runCommand({ compact: collection });
  });
"

# Update statistics
docker exec ecommerce-mongodb mongosh --eval "
  db.getSiblingDB('ProductDb').runCommand({ reIndex: 'products' });
"
```

### Log Rotation

**Logrotate Configuration:**
```bash
# /etc/logrotate.d/ecommerce

/var/log/ecommerce/*.log {
    daily
    rotate 7
    compress
    delaycompress
    notifempty
    create 0640 www-data www-data
    sharedscripts
    postrotate
        docker exec ecommerce-api-gateway kill -USR1 1
    endscript
}
```

### Certificate Renewal

**Let's Encrypt with Cert-Manager:**
```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@ecommerce.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

**Manual Certificate Update:**
```bash
# Generate new certificate
certbot certonly --manual --preferred-challenges dns -d api.ecommerce.com

# Update Kubernetes secret
kubectl create secret tls ecommerce-tls \
  --cert=/etc/letsencrypt/live/api.ecommerce.com/fullchain.pem \
  --key=/etc/letsencrypt/live/api.ecommerce.com/privkey.pem \
  --namespace=ecommerce \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart ingress controller
kubectl rollout restart deployment/nginx-ingress-controller -n ingress-nginx
```

---

## Cost Optimization

### Resource Right-Sizing

```bash
# Analyze resource usage
kubectl top pods -n ecommerce --sort-by=memory
kubectl top pods -n ecommerce --sort-by=cpu

# Get resource recommendations
kubectl describe vpa user-service-vpa -n ecommerce
```

### Auto-Scaling Configuration

```yaml
# Cluster Autoscaler
apiVersion: v1
kind: ConfigMap
metadata:
  name: cluster-autoscaler
  namespace: kube-system
data:
  scale-down-enabled: "true"
  scale-down-delay-after-add: "10m"
  scale-down-unneeded-time: "10m"
  skip-nodes-with-local-storage: "false"
  skip-nodes-with-system-pods: "true"
```

### Cost Monitoring Script

```bash
#!/bin/bash
# cost-report.sh

echo "E-Commerce Platform Cost Report"
echo "================================"

# Get resource usage
echo "\nPod Resource Usage:"
kubectl top pods -n ecommerce --no-headers | awk '{print $1, $2, $3}'

# Calculate estimated costs
echo "\nEstimated Monthly Costs:"
echo "Compute: \$XXX"
echo "Storage: \$XXX"
echo "Network: \$XXX"
echo "Total: \$XXX"

# Identify optimization opportunities
echo "\nOptimization Opportunities:"
kubectl get pods -n ecommerce -o json | \
  jq -r '.items[] | select(.spec.containers[].resources.requests.memory == null) | .metadata.name' | \
  while read pod; do
    echo "- $pod has no resource requests defined"
  done
```

---

## Documentation and Runbooks

### Incident Response Runbook

**Service Down Runbook:**
```markdown
# Service Down Incident Response

## Symptoms
- Health check failing
- 5xx errors increasing
- Service not responding

## Investigation Steps
1. Check pod status: `kubectl get pods -n ecommerce`
2. Check logs: `kubectl logs -f <pod-name> -n ecommerce`
3. Check events: `kubectl get events -n ecommerce --sort-by='.lastTimestamp'`
4. Check resource usage: `kubectl top pods -n ecommerce`

## Common Causes
- OOM (Out of Memory)
- Database connection issues
- Configuration errors
- Resource exhaustion

## Resolution Steps
1. Restart pod: `kubectl delete pod <pod-name> -n ecommerce`
2. Scale up if needed: `kubectl scale deployment/<service> --replicas=5 -n ecommerce`
3. Check database: Verify database is accessible
4. Review recent deployments: `kubectl rollout history deployment/<service> -n ecommerce`

## Escalation
If issue persists > 15 minutes:
- Page on-call engineer
- Notify #incidents channel
- Consider rollback
```

### Knowledge Base

Create documentation for:
- [ ] Architecture diagrams
- [ ] API documentation
- [ ] Deployment procedures
- [ ] Troubleshooting guides
- [ ] Runbooks for common incidents
- [ ] Onboarding guide for new developers
- [ ] Security procedures
- [ ] Disaster recovery procedures

---

## Final Notes

### Support Resources

**Documentation:**
- Architecture Docs: `/docs/architecture.md`
- API Docs: `/docs/api-documentation.md`
- Deployment Guide: `/docs/deployment-guide.md`

**Tools:**
- Monitoring: http://grafana.ecommerce.com
- Logs: http://seq.ecommerce.com
- Metrics: http://prometheus.ecommerce.com
- Tracing: http://jaeger.ecommerce.com

**Communication:**
- Slack: #ecommerce-platform
- Email: platform-team@ecommerce.com
- On-Call: PagerDuty rotation

**Emergency Contacts:**
- Platform Lead: +1-XXX-XXX-XXXX
- DevOps Lead: +1-XXX-XXX-XXXX
- Database Admin: +1-XXX-XXX-XXXX

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Maintained By**: Platform Team  
**Review Schedule**: Quarterly