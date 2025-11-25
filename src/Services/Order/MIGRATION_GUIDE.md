# Order Service - Migration Guide

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 16
- EF Core CLI tools

## Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef
```

## Database Setup

### Option 1: Docker (Recommended)

```bash
# Start PostgreSQL
docker run -d --name postgres-order \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=ecommerce_order \
  -p 5432:5432 \
  postgres:16-alpine

# Verify it's running
docker ps | grep postgres-order
```

### Option 2: Existing PostgreSQL

Create database manually:
```sql
CREATE DATABASE ecommerce_order;
```

## Create and Apply Migrations

### 1. Navigate to API Project

```bash
cd src/Services/Order/ECommerce.Order.API
```

### 2. Create Initial Migration

```bash
dotnet ef migrations add InitialCreate \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project . \
  --output-dir Data/Migrations
```

### 3. Review Migration

Check the generated migration file in:
`ECommerce.Order.Infrastructure/Data/Migrations/`

### 4. Apply Migration

```bash
dotnet ef database update \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project .
```

### 5. Verify Database

```bash
# Connect to PostgreSQL
docker exec -it postgres-order psql -U postgres -d ecommerce_order

# List tables
\dt

# Expected tables:
# - orders
# - order_items
# - order_status_history
# - __EFMigrationsHistory

# View schema
\d orders
\d order_items
\d order_status_history

# Exit
\q
```

## Common Commands

### Create New Migration

```bash
dotnet ef migrations add MigrationName \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project .
```

### Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project .
```

### Rollback to Specific Migration

```bash
dotnet ef database update MigrationName \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project .
```

### Generate SQL Script

```bash
dotnet ef migrations script \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project . \
  --output migration.sql
```

### Drop Database (Careful!)

```bash
dotnet ef database drop \
  --project ../ECommerce.Order.Infrastructure \
  --startup-project . \
  --force
```

## Troubleshooting

### Error: "No DbContext was found"

Make sure you're in the API project directory:
```bash
cd src/Services/Order/ECommerce.Order.API
```

### Error: "Connection refused"

Check PostgreSQL is running:
```bash
docker ps | grep postgres
```

### Error: "Database already exists"

Drop and recreate:
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Error: "Build failed"

Restore packages first:
```bash
dotnet restore
dotnet build
```

## Database Schema

### orders Table

```sql
CREATE TABLE orders (
    id uuid PRIMARY KEY,
    order_number varchar(50) UNIQUE NOT NULL,
    user_id uuid NOT NULL,
    status int NOT NULL,
    payment_status int NOT NULL,
    payment_method int NOT NULL,
    subtotal decimal(18,2) NOT NULL,
    shipping_cost decimal(18,2) NOT NULL,
    tax decimal(18,2) NOT NULL,
    discount decimal(18,2) NOT NULL,
    total_amount decimal(18,2) NOT NULL,
    currency varchar(3) DEFAULT 'USD',
    shipping_full_name varchar(100) NOT NULL,
    shipping_phone varchar(20) NOT NULL,
    shipping_address_line1 varchar(200) NOT NULL,
    shipping_address_line2 varchar(200),
    shipping_city varchar(100) NOT NULL,
    shipping_state varchar(100) NOT NULL,
    shipping_postal_code varchar(20) NOT NULL,
    shipping_country varchar(100) NOT NULL,
    tracking_number varchar(100),
    payment_transaction_id varchar(100),
    shipped_at timestamp,
    delivered_at timestamp,
    cancelled_at timestamp,
    cancellation_reason varchar(500),
    customer_notes varchar(500),
    admin_notes varchar(500),
    paid_at timestamp,
    created_at timestamp NOT NULL,
    updated_at timestamp,
    created_by varchar(100),
    updated_by varchar(100)
);

CREATE INDEX idx_orders_order_number ON orders(order_number);
CREATE INDEX idx_orders_user_id ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_created_at ON orders(created_at);
```

### order_items Table

```sql
CREATE TABLE order_items (
    id uuid PRIMARY KEY,
    order_id uuid NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id varchar(50) NOT NULL,
    product_name varchar(200) NOT NULL,
    sku varchar(50) NOT NULL,
    image_url varchar(500),
    unit_price decimal(18,2) NOT NULL,
    quantity int NOT NULL,
    discount decimal(18,2) NOT NULL,
    total_price decimal(18,2) NOT NULL,
    created_at timestamp NOT NULL,
    updated_at timestamp,
    created_by varchar(100),
    updated_by varchar(100)
);

CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_order_items_product_id ON order_items(product_id);
```

### order_status_history Table

```sql
CREATE TABLE order_status_history (
    id uuid PRIMARY KEY,
    order_id uuid NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    status int NOT NULL,
    notes varchar(500),
    changed_by varchar(100),
    changed_at timestamp NOT NULL,
    created_at timestamp NOT NULL,
    updated_at timestamp,
    created_by varchar(100),
    updated_by varchar(100)
);

CREATE INDEX idx_order_status_history_order_id ON order_status_history(order_id);
CREATE INDEX idx_order_status_history_changed_at ON order_status_history(changed_at);
```

## Next Steps

After successful migration:

1. ✅ Run the API: `dotnet run`
2. ✅ Open Swagger: http://localhost:5003
3. ✅ Test endpoints with `order-service.http`
4. ⏳ Integrate with ShoppingCart Service
5. ⏳ Add authentication
