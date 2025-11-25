# gRPC Implementation Guide

## Overview

This guide explains how to implement gRPC for service-to-service communication in the ECommerce microservices architecture.

## Why gRPC?

- **Performance:** 7-10x faster than REST
- **Binary Protocol:** Smaller payload size
- **Strongly Typed:** Proto files as contracts
- **Bi-directional Streaming:** Real-time communication
- **HTTP/2:** Multiplexing, header compression

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT (Web/Mobile)                   │
└────────────────────────┬────────────────────────────────┘
                         │ REST/HTTP
                         ▼
                ┌─────────────────┐
                │  API Gateway    │
                │  (Ocelot)       │
                └────────┬────────┘
                         │ REST/HTTP (forward)
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ User Service │  │Product Service│  │ Order Service│
│              │  │               │  │              │
│ REST: 5000   │  │ REST: 5001    │  │ REST: 5003   │
│ gRPC: 5010   │  │ gRPC: 5011    │  │ gRPC: 5013   │
└──────┬───────┘  └──────┬────────┘  └──────┬───────┘
       │                 │                  │
       └─────────────────┼──────────────────┘
                         │
                    gRPC (internal)
                    (service-to-service)
```

## Services Priority

### High Priority (Implement First)

1. **Product Service gRPC** - Most called service
   - GetProductInfo
   - CheckStock
   - ValidateProducts
   - GetProductsBatch

2. **User Service gRPC** - Authentication & user info
   - GetUserInfo
   - ValidateUser
   - GetUserAddresses

3. **Order Service gRPC** - For Payment Service
   - GetOrderInfo
   - UpdateOrderStatus
   - ValidateOrder

### Low Priority (Optional)

4. **Payment Service gRPC** - For Order Service
   - GetPaymentStatus
   - ProcessPayment

5. **ShoppingCart Service** - Usually accessed via REST only

6. **Notification Service** - Event-driven, no gRPC needed

---

## Implementation Steps

### Step 1: Product Service gRPC

#### 1.1. Create Proto File

**File:** `src/Services/Product/ECommerce.Product.API/Protos/product.proto`

```protobuf
syntax = "proto3";

option csharp_namespace = "ECommerce.Product.Grpc";

package product;

service ProductGrpcService {
  rpc GetProductInfo (ProductInfoRequest) returns (ProductInfoResponse);
  rpc CheckStock (StockCheckRequest) returns (StockCheckResponse);
  rpc ValidateProducts (ValidateProductsRequest) returns (ValidateProductsResponse);
}

message ProductInfoRequest {
  string product_id = 1;
}

message ProductInfoResponse {
  string id = 1;
  string name = 2;
  string sku = 3;
  double price = 4;
  int32 stock_quantity = 5;
  bool is_available = 6;
  string image_url = 7;
  string currency = 8;
}

message StockCheckRequest {
  string product_id = 1;
  int32 quantity = 2;
}

message StockCheckResponse {
  bool available = 1;
  int32 stock_quantity = 2;
  string message = 3;
}

message ValidateProductsRequest {
  repeated ProductValidationItem items = 1;
}

message ProductValidationItem {
  string product_id = 1;
  int32 quantity = 2;
}

message ValidateProductsResponse {
  bool all_valid = 1;
  repeated ProductValidationResult results = 2;
}

message ProductValidationResult {
  string product_id = 1;
  bool valid = 2;
  string message = 3;
  int32 available_quantity = 4;
}
```

#### 1.2. Update csproj

```xml
<ItemGroup>
  <PackageReference Include="Grpc.AspNetCore" Version="2.66.0" />
</ItemGroup>

<ItemGroup>
  <Protobuf Include="Protos\product.proto" GrpcServices="Server" />
</ItemGroup>
```

#### 1.3. Implement gRPC Service

**File:** `src/Services/Product/ECommerce.Product.API/Grpc/ProductGrpcService.cs`

```csharp
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Grpc;
using Grpc.Core;

public class ProductGrpcService : Product.Grpc.ProductGrpcService.ProductGrpcServiceBase
{
    private readonly IProductService _productService;
    
    public override async Task<ProductInfoResponse> GetProductInfo(
        ProductInfoRequest request, 
        ServerCallContext context)
    {
        var product = await _productService.GetByIdAsync(request.ProductId);
        
        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
        }

        return new ProductInfoResponse
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.SKU,
            Price = (double)product.Price,
            StockQuantity = product.StockQuantity,
            IsAvailable = product.IsAvailable,
            ImageUrl = product.Images?.FirstOrDefault()?.Url ?? "",
            Currency = "VND"
        };
    }
    
    // Implement other methods...
}
```

#### 1.4. Register in Program.cs

```csharp
// Add gRPC
builder.Services.AddGrpc();

// Configure Kestrel for multiple ports
builder.WebHost.ConfigureKestrel(options =>
{
    // REST API
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http1);
    // gRPC
    options.ListenLocalhost(5011, o => o.Protocols = HttpProtocols.Http2);
});

// Map gRPC service
app.MapGrpcService<ProductGrpcService>();
```

---

### Step 2: Order Service - Use Product gRPC Client

#### 2.1. Add gRPC Client Package

```xml
<ItemGroup>
  <PackageReference Include="Grpc.Net.Client" Version="2.66.0" />
  <PackageReference Include="Google.Protobuf" Version="3.28.3" />
  <PackageReference Include="Grpc.Tools" Version="2.66.0" PrivateAssets="All" />
</ItemGroup>

<ItemGroup>
  <Protobuf Include="Protos\product.proto" GrpcServices="Client" />
</ItemGroup>
```

#### 2.2. Copy Proto File

Copy `product.proto` from Product Service to Order Service `Protos/` folder.

#### 2.3. Create gRPC Client Service

```csharp
public interface IProductGrpcClient
{
    Task<ProductInfoResponse> GetProductInfoAsync(string productId);
    Task<StockCheckResponse> CheckStockAsync(string productId, int quantity);
}

public class ProductGrpcClient : IProductGrpcClient
{
    private readonly ProductGrpcService.ProductGrpcServiceClient _client;
    
    public ProductGrpcClient(GrpcChannel channel)
    {
        _client = new ProductGrpcService.ProductGrpcServiceClient(channel);
    }
    
    public async Task<ProductInfoResponse> GetProductInfoAsync(string productId)
    {
        var request = new ProductInfoRequest { ProductId = productId };
        return await _client.GetProductInfoAsync(request);
    }
    
    public async Task<StockCheckResponse> CheckStockAsync(string productId, int quantity)
    {
        var request = new StockCheckRequest 
        { 
            ProductId = productId, 
            Quantity = quantity 
        };
        return await _client.CheckStockAsync(request);
    }
}
```

#### 2.4. Register gRPC Client

```csharp
// Register gRPC Channel
builder.Services.AddSingleton(services =>
{
    var productServiceUrl = builder.Configuration["Services:Product:GrpcUrl"] 
        ?? "http://localhost:5011";
    return GrpcChannel.ForAddress(productServiceUrl);
});

// Register gRPC Client
builder.Services.AddScoped<IProductGrpcClient, ProductGrpcClient>();
```

#### 2.5. Use in Order Service

```csharp
public class OrderService : IOrderService
{
    private readonly IProductGrpcClient _productGrpcClient;
    
    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        // Validate products via gRPC (fast!)
        foreach (var item in cart.Items)
        {
            var stockCheck = await _productGrpcClient.CheckStockAsync(
                item.ProductId, 
                item.Quantity
            );
            
            if (!stockCheck.Available)
            {
                throw new InvalidOperationException(stockCheck.Message);
            }
        }
        
        // Create order...
    }
}
```

---

## Configuration

### appsettings.json

```json
{
  "Services": {
    "Product": {
      "RestUrl": "http://localhost:5001",
      "GrpcUrl": "http://localhost:5011"
    },
    "User": {
      "RestUrl": "http://localhost:5000",
      "GrpcUrl": "http://localhost:5010"
    },
    "Order": {
      "RestUrl": "http://localhost:5003",
      "GrpcUrl": "http://localhost:5013"
    }
  }
}
```

---

## API Gateway Routes

Update `ocelot.Development.json`:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5000 }],
      "UpstreamPathTemplate": "/auth/{everything}",
      "UpstreamHttpMethod": [ "POST" ]
    },
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5000 }],
      "UpstreamPathTemplate": "/users/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5001 }],
      "UpstreamPathTemplate": "/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ]
    },
    {
      "DownstreamPathTemplate": "/api/cart/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5002 }],
      "UpstreamPathTemplate": "/cart/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5003 }],
      "UpstreamPathTemplate": "/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "DownstreamPathTemplate": "/api/payments/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5004 }],
      "UpstreamPathTemplate": "/payments/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE", "PATCH" ],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5050"
  }
}
```

---

## Testing

### Test gRPC Endpoint

Use **grpcurl** or **Postman** (supports gRPC):

```bash
# Install grpcurl
go install github.com/fullstorydev/grpcurl/cmd/grpcurl@latest

# List services
grpcurl -plaintext localhost:5011 list

# Call method
grpcurl -plaintext -d '{"product_id": "prod-001"}' \
  localhost:5011 product.ProductGrpcService/GetProductInfo
```

### Test via Order Service

```bash
# Create order (internally calls Product gRPC)
POST http://localhost:5003/api/orders
# → Order Service validates products via gRPC
# → Much faster than REST!
```

---

## Performance Comparison

### REST API
```
Order Service → HTTP GET http://localhost:5001/api/products/prod-001
Response time: ~50ms
Payload size: ~2KB (JSON)
```

### gRPC
```
Order Service → gRPC localhost:5011 GetProductInfo(prod-001)
Response time: ~5ms (10x faster!)
Payload size: ~200 bytes (Protobuf, 10x smaller!)
```

---

## Benefits

1. **Performance:** 7-10x faster than REST
2. **Type Safety:** Proto files as contracts
3. **Smaller Payload:** Binary protocol
4. **Better for Internal:** Service-to-service communication
5. **Streaming:** Bi-directional streaming support

---

## When to Use

### Use gRPC for:
- ✅ Service-to-service communication
- ✅ High-frequency calls
- ✅ Performance-critical operations
- ✅ Internal microservices

### Use REST for:
- ✅ Client-facing APIs (Web/Mobile)
- ✅ Public APIs
- ✅ Low-frequency calls
- ✅ External integrations

---

## Summary

- **Product Service:** Expose gRPC (port 5011) + REST (port 5001)
- **User Service:** Expose gRPC (port 5010) + REST (port 5000)
- **Order Service:** Expose gRPC (port 5013) + REST (port 5003) + Use Product/User gRPC clients
- **Payment Service:** Use Order gRPC client
- **API Gateway:** Route REST only (clients don't use gRPC)

**Result:** Faster, more efficient service-to-service communication! 🚀
