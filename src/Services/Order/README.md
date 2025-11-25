# Order Service

Order management microservice for ECommerce Platform.

## Features

- ✅ Create order from shopping cart
- ✅ Order status management (Pending → Processing → Shipped → Delivered)
- ✅ Order history and tracking
- ✅ Payment status tracking
- ✅ Shipping address management
- ✅ Order cancellation
- ✅ Event-driven architecture (OrderCreatedEvent)
- ✅ Integration with ShoppingCart Service
- ✅ PostgreSQL database with EF Core

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Event Bus:** In-Memory (ready for RabbitMQ)

## Architecture

```
Order/
├── Domain/          # Entities, Enums, Interfaces
├── Application/     # Services, DTOs, Validators
├── Infrastructure/  # Repositories, DbContext, External Services
└── API/            # Controllers, Program.cs
```

## Quick Start

### 1. Setup Database

```bash
# Start PostgreSQL
docker run -d --name postgres-order \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=ecommerce_order \
  -p 5432:5432 \
  postgres:16-alpine

# Or use existing docker-compose
cd docker
docker-compose -f docker-compose.infrastructure.yml up postgres -d
```

### 2. Run Migrations

```bash
cd src/Services/Order/ECommerce.Order.API

# Create migration
dotnet ef migrations add InitialCreate --project ../ECommerce.Order.Infrastructure

# Apply migration
dotnet ef database update --project ../ECommerce.Order.Infrastructure
```

### 3. Run API

```bash
dotnet run --urls "http://localhost:5003"
```

### 4. Open Swagger

http://localhost:5003

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/orders` | Create order from cart | User |
| GET | `/api/orders/{id}` | Get order by ID | User |
| GET | `/api/orders/number/{orderNumber}` | Get by order number | User |
| GET | `/api/orders/my-orders` | Get user's orders | User |
| GET | `/api/orders` | Get all orders | Admin |
| GET | `/api/orders/status/{status}` | Get by status | Admin |
| PATCH | `/api/orders/{id}/status` | Update status | Admin |
| POST | `/api/orders/{id}/cancel` | Cancel order | User |

## Order Status Flow

```
Pending → Processing → Shipped → Delivered
   ↓           ↓
Cancelled  Cancelled
```

## Database Schema

### orders
- id (uuid, PK)
- order_number (varchar, unique)
- user_id (uuid)
- status (int)
- payment_status (int)
- payment_method (int)
- subtotal, shipping_cost, tax, discount, total_amount (decimal)
- shipping address fields
- tracking_number
- timestamps

### order_items
- id (uuid, PK)
- order_id (uuid, FK)
- product_id (varchar)
- product_name, sku, image_url
- unit_price, quantity, discount, total_price (decimal)

### order_status_history
- id (uuid, PK)
- order_id (uuid, FK)
- status (int)
- notes, changed_by
- changed_at (timestamp)

## Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce_order;Username=postgres;Password=postgres"
  },
  "Services": {
    "ShoppingCart": "http://localhost:5002"
  }
}
```

## Integration

### With ShoppingCart Service
- Get cart items when creating order
- Clear cart after successful order creation

### With Product Service (Future)
- Validate product availability
- Update stock quantities

### Event Publishing
- `OrderCreatedEvent` - Published when order is created
- Can be consumed by Payment, Notification, Inventory services

## Testing

See `order-service.http` for API test scenarios.

## Next Steps

1. ✅ Run migrations
2. ✅ Test API endpoints
3. ⏳ Integrate JWT authentication
4. ⏳ Add payment processing
5. ⏳ Add email notifications
6. ⏳ Add order tracking
