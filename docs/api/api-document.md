# E-Commerce Platform - API Documentation

## Table of Contents
1. [API Overview](#api-overview)
2. [Authentication](#authentication)
3. [User Service API](#user-service-api)
4. [Product Catalog Service API](#product-catalog-service-api)
5. [Shopping Cart Service API](#shopping-cart-service-api)
6. [Order Service API](#order-service-api)
7. [Payment Service API](#payment-service-api)
8. [Notification Service API](#notification-service-api)
9. [Error Handling](#error-handling)
10. [Rate Limiting](#rate-limiting)
11. [Webhooks](#webhooks)

---

## API Overview

### Base URL
```
Development: http://localhost:5000/api
Production:  https://api.ecommerce.com/api
```

### API Versioning
All APIs are versioned using URL path versioning:
```
/api/v1/users
/api/v2/users
```

### Common Headers
```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer {access_token}
X-Correlation-Id: {unique-request-id}
X-API-Version: v1
```

### Standard Response Format

**Success Response:**
```json
{
  "success": true,
  "data": {
    // Response data
  },
  "message": "Operation completed successfully",
  "timestamp": "2025-11-02T10:30:00Z"
}
```

**Error Response:**
```json
{
  "success": false,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Invalid email format",
      "field": "email"
    }
  ],
  "message": "Request validation failed",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "abc123-def456-ghi789"
}
```

### Pagination
```json
{
  "success": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 95,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

## Authentication

### OAuth 2.0 / OpenID Connect Flow

#### 1. Register User
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "emailVerificationRequired": true
  },
  "message": "Registration successful. Please verify your email."
}
```

#### 2. Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "roles": ["Customer"]
    }
  }
}
```

#### 3. Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "new_access_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

#### 4. Logout
```http
POST /api/v1/auth/logout
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "refreshToken": "refresh_token_here"
}
```

**Response (204 No Content)**

---

## User Service API

### Base Path: `/api/v1/users`

### Get User Profile
```http
GET /api/v1/users/profile
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "dateOfBirth": "1990-01-15",
    "addresses": [
      {
        "id": "addr-001",
        "street": "123 Main St",
        "city": "New York",
        "state": "NY",
        "zipCode": "10001",
        "country": "USA",
        "isDefault": true
      }
    ],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2025-11-02T10:30:00Z"
  }
}
```

### Update User Profile
```http
PUT /api/v1/users/profile
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "dateOfBirth": "1990-01-15"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "phoneNumber": "+1234567890",
    "dateOfBirth": "1990-01-15"
  },
  "message": "Profile updated successfully"
}
```

### Change Password
```http
POST /api/v1/users/change-password
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

### Forgot Password
```http
POST /api/v1/users/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Password reset link sent to your email"
}
```

### Reset Password
```http
POST /api/v1/users/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "NewPass789!",
  "confirmPassword": "NewPass789!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Password reset successfully"
}
```

### Manage Addresses

#### Add Address
```http
POST /api/v1/users/addresses
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "street": "456 Oak Ave",
  "city": "Los Angeles",
  "state": "CA",
  "zipCode": "90001",
  "country": "USA",
  "isDefault": false
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "addr-002",
    "street": "456 Oak Ave",
    "city": "Los Angeles",
    "state": "CA",
    "zipCode": "90001",
    "country": "USA",
    "isDefault": false
  }
}
```

#### Update Address
```http
PUT /api/v1/users/addresses/{addressId}
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "street": "456 Oak Avenue",
  "city": "Los Angeles",
  "state": "CA",
  "zipCode": "90001",
  "country": "USA",
  "isDefault": true
}
```

#### Delete Payment Method
```http
DELETE /api/v1/payments/methods/{methodId}
Authorization: Bearer {access_token}
```

**Response (204 No Content)**

---

## Notification Service API

### Base Path: `/api/v1/notifications`

### Send Email Notification
```http
POST /api/v1/notifications/email
Authorization: Bearer {service_access_token}
Content-Type: application/json

{
  "to": "user@example.com",
  "subject": "Order Confirmation",
  "templateId": "order-confirmation",
  "templateData": {
    "orderNumber": "ORD-2025-000100",
    "customerName": "John Doe",
    "orderTotal": 863.98,
    "orderItems": [...]
  }
}
```

**Response (202 Accepted):**
```json
{
  "success": true,
  "data": {
    "notificationId": "notif-123",
    "status": "queued",
    "queuedAt": "2025-11-02T10:30:00Z"
  },
  "message": "Email notification queued for delivery"
}
```

### Send SMS Notification
```http
POST /api/v1/notifications/sms
Authorization: Bearer {service_access_token}
Content-Type: application/json

{
  "to": "+1234567890",
  "message": "Your order ORD-2025-000100 has been shipped!",
  "templateId": "order-shipped"
}
```

**Response (202 Accepted):**
```json
{
  "success": true,
  "data": {
    "notificationId": "notif-124",
    "status": "queued",
    "queuedAt": "2025-11-02T10:30:00Z"
  }
}
```

### Get Notification History
```http
GET /api/v1/notifications/history?pageNumber=1&pageSize=20&type=email
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 20)
- `type` (optional: email, sms, push)
- `status` (optional: queued, sent, failed, delivered)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "notif-123",
        "type": "email",
        "to": "user@example.com",
        "subject": "Order Confirmation",
        "status": "delivered",
        "queuedAt": "2025-11-02T10:30:00Z",
        "sentAt": "2025-11-02T10:30:15Z",
        "deliveredAt": "2025-11-02T10:30:20Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 156
  }
}
```

### Get Notification Status
```http
GET /api/v1/notifications/{notificationId}
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "notif-123",
    "type": "email",
    "to": "user@example.com",
    "subject": "Order Confirmation",
    "status": "delivered",
    "attempts": 1,
    "queuedAt": "2025-11-02T10:30:00Z",
    "sentAt": "2025-11-02T10:30:15Z",
    "deliveredAt": "2025-11-02T10:30:20Z",
    "error": null
  }
}
```

---

## Error Handling

### HTTP Status Codes

| Status Code | Description |
|------------|-------------|
| 200 | OK - Request succeeded |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request succeeded with no content to return |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Authentication required or failed |
| 403 | Forbidden - Authenticated but not authorized |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource conflict (e.g., duplicate) |
| 422 | Unprocessable Entity - Validation error |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |
| 503 | Service Unavailable - Service temporarily unavailable |

### Error Response Format

```json
{
  "success": false,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Email is required",
      "field": "email"
    },
    {
      "code": "INVALID_FORMAT",
      "message": "Invalid email format",
      "field": "email"
    }
  ],
  "message": "Request validation failed",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "abc123-def456-ghi789",
  "path": "/api/v1/users/register"
}
```

### Common Error Codes

| Error Code | Description |
|-----------|-------------|
| VALIDATION_ERROR | Input validation failed |
| INVALID_FORMAT | Invalid data format |
| REQUIRED_FIELD | Required field missing |
| UNAUTHORIZED | Authentication required |
| FORBIDDEN | Insufficient permissions |
| NOT_FOUND | Resource not found |
| ALREADY_EXISTS | Resource already exists |
| CONFLICT | Resource conflict |
| EXPIRED_TOKEN | Token has expired |
| INVALID_TOKEN | Invalid or malformed token |
| INSUFFICIENT_STOCK | Not enough items in stock |
| PAYMENT_FAILED | Payment processing failed |
| RATE_LIMIT_EXCEEDED | Too many requests |
| SERVICE_UNAVAILABLE | Service temporarily unavailable |
| INTERNAL_ERROR | Internal server error |

### Example Error Scenarios

#### Validation Error (400)
```json
{
  "success": false,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Password must be at least 8 characters",
      "field": "password"
    },
    {
      "code": "INVALID_FORMAT",
      "message": "Email format is invalid",
      "field": "email"
    }
  ],
  "message": "Validation failed",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "abc123"
}
```

#### Authentication Error (401)
```json
{
  "success": false,
  "errors": [
    {
      "code": "UNAUTHORIZED",
      "message": "Invalid credentials"
    }
  ],
  "message": "Authentication failed",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "def456"
}
```

#### Authorization Error (403)
```json
{
  "success": false,
  "errors": [
    {
      "code": "FORBIDDEN",
      "message": "You do not have permission to access this resource"
    }
  ],
  "message": "Access denied",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "ghi789"
}
```

#### Not Found Error (404)
```json
{
  "success": false,
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "Product with ID 'prod-999' not found"
    }
  ],
  "message": "Resource not found",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "jkl012"
}
```

#### Stock Error (409)
```json
{
  "success": false,
  "errors": [
    {
      "code": "INSUFFICIENT_STOCK",
      "message": "Only 5 items available, requested 10",
      "field": "quantity",
      "availableStock": 5,
      "requestedQuantity": 10
    }
  ],
  "message": "Insufficient stock",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "mno345"
}
```

#### Rate Limit Error (429)
```json
{
  "success": false,
  "errors": [
    {
      "code": "RATE_LIMIT_EXCEEDED",
      "message": "Rate limit exceeded. Try again in 60 seconds"
    }
  ],
  "message": "Too many requests",
  "timestamp": "2025-11-02T10:30:00Z",
  "traceId": "pqr678",
  "retryAfter": 60
}
```

---

## Rate Limiting

### Rate Limit Headers

All API responses include rate limiting information:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1698930000
```

### Rate Limits by Plan

| Plan | Requests per Hour | Burst Limit |
|------|------------------|-------------|
| Guest | 100 | 10 |
| Customer | 1000 | 50 |
| Premium | 5000 | 100 |
| Admin | 10000 | 200 |

### Rate Limit Exceeded Response

```json
{
  "success": false,
  "errors": [
    {
      "code": "RATE_LIMIT_EXCEEDED",
      "message": "API rate limit exceeded"
    }
  ],
  "message": "Too many requests",
  "retryAfter": 3600,
  "timestamp": "2025-11-02T10:30:00Z"
}
```

---

## Webhooks

### Webhook Events

The platform can send webhooks for the following events:

| Event | Description |
|-------|-------------|
| order.created | New order created |
| order.paid | Order payment confirmed |
| order.shipped | Order has been shipped |
| order.delivered | Order has been delivered |
| order.cancelled | Order has been cancelled |
| payment.succeeded | Payment succeeded |
| payment.failed | Payment failed |
| payment.refunded | Payment refunded |
| product.created | New product created |
| product.updated | Product updated |
| product.deleted | Product deleted |
| user.created | New user registered |

### Webhook Registration

```http
POST /api/v1/webhooks
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "url": "https://your-domain.com/webhooks/ecommerce",
  "events": [
    "order.created",
    "order.paid",
    "order.shipped"
  ],
  "secret": "your_webhook_secret"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "webhook-001",
    "url": "https://your-domain.com/webhooks/ecommerce",
    "events": [
      "order.created",
      "order.paid",
      "order.shipped"
    ],
    "status": "active",
    "createdAt": "2025-11-02T10:30:00Z"
  }
}
```

### Webhook Payload Format

```json
{
  "id": "evt_123456",
  "type": "order.created",
  "createdAt": "2025-11-02T10:30:00Z",
  "data": {
    "object": {
      "id": "order-100",
      "orderNumber": "ORD-2025-000100",
      "userId": "user-123",
      "status": "pending",
      "total": 863.98,
      "items": [...]
    }
  }
}
```

### Webhook Signature Verification

Each webhook includes a signature header:

```http
X-Webhook-Signature: sha256=abc123def456...
```

**Verification Example (C#):**
```csharp
public bool VerifyWebhookSignature(string payload, string signature, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    var computedSignature = $"sha256={BitConverter.ToString(hash).Replace("-", "").ToLower()}";
    
    return signature == computedSignature;
}
```

### Webhook Retry Policy

- **Maximum Retries**: 5
- **Retry Intervals**: 1m, 5m, 30m, 1h, 3h
- **Timeout**: 30 seconds
- **Success Codes**: 200-299

---

## API Best Practices

### 1. Always Include Authorization Header
```http
Authorization: Bearer {access_token}
```

### 2. Use Correlation IDs for Tracing
```http
X-Correlation-Id: unique-request-id
```

### 3. Handle Pagination
```http
GET /api/v1/products?pageNumber=1&pageSize=20
```

### 4. Implement Retry Logic with Exponential Backoff
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

### 5. Cache Responses When Appropriate
```http
Cache-Control: public, max-age=3600
ETag: "abc123"
```

### 6. Use Idempotency Keys for Critical Operations
```http
POST /api/v1/payments/process
Idempotency-Key: unique-operation-id
```

### 7. Validate Input on Client Side
Before making API calls, validate:
- Required fields
- Data formats
- Business rules

### 8. Handle Errors Gracefully
```javascript
try {
  const response = await fetch('/api/v1/products');
  if (!response.ok) {
    const error = await response.json();
    handleError(error);
  }
  const data = await response.json();
  return data;
} catch (error) {
  console.error('API Error:', error);
  showUserFriendlyError();
}
```

---

## API Client Examples

### cURL
```bash
# Login
curl -X POST https://api.ecommerce.com/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Get Products
curl -X GET "https://api.ecommerce.com/api/v1/products?pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer {access_token}"

# Create Order
curl -X POST https://api.ecommerce.com/api/v1/orders \
  -H "Authorization: Bearer {access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "shippingAddressId": "addr-001",
    "billingAddressId": "addr-001",
    "paymentMethodId": "pm-001"
  }'
```

### JavaScript/TypeScript (Axios)
```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.ecommerce.com/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add token to requests
api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Login
const login = async (email: string, password: string) => {
  const response = await api.post('/v1/auth/login', {
    email,
    password
  });
  return response.data;
};

// Get Products
const getProducts = async (pageNumber = 1, pageSize = 20) => {
  const response = await api.get('/v1/products', {
    params: { pageNumber, pageSize }
  });
  return response.data;
};

// Create Order
const createOrder = async (orderData: CreateOrderDto) => {
  const response = await api.post('/v1/orders', orderData);
  return response.data;
};
```

### C# (.NET)
```csharp
public class ECommerceApiClient
{
    private readonly HttpClient _httpClient;
    private string _accessToken;

    public ECommerceApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        _accessToken = result.Data.AccessToken;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
        
        return result.Data;
    }

    public async Task<PagedResult<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 20)
    {
        var response = await _httpClient.GetAsync(
            $"/api/v1/products?pageNumber={pageNumber}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<Product>>>();
        return result.Data;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/orders", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Order>>();
        return result.Data;
    }
}
```

### Python (Requests)
```python
import requests
from typing import Dict, Any

class ECommerceApiClient:
    def __init__(self, base_url: str):
        self.base_url = base_url
        self.session = requests.Session()
        self.session.headers.update({'Content-Type': 'application/json'})
    
    def login(self, email: str, password: str) -> Dict[str, Any]:
        response = self.session.post(
            f'{self.base_url}/api/v1/auth/login',
            json={'email': email, 'password': password}
        )
        response.raise_for_status()
        data = response.json()
        
        # Set token for future requests
        self.session.headers.update({
            'Authorization': f"Bearer {data['data']['accessToken']}"
        })
        
        return data['data']
    
    def get_products(self, page_number: int = 1, page_size: int = 20) -> Dict[str, Any]:
        response = self.session.get(
            f'{self.base_url}/api/v1/products',
            params={'pageNumber': page_number, 'pageSize': page_size}
        )
        response.raise_for_status()
        return response.json()['data']
    
    def create_order(self, order_data: Dict[str, Any]) -> Dict[str, Any]:
        response = self.session.post(
            f'{self.base_url}/api/v1/orders',
            json=order_data
        )
        response.raise_for_status()
        return response.json()['data']
```

---

## Testing APIs

### Postman Collection
Import the provided Postman collection for easy API testing:
- [Download Postman Collection](./postman/ecommerce-api.json)

### Environment Variables
```json
{
  "baseUrl": "http://localhost:5000/api",
  "accessToken": "",
  "refreshToken": "",
  "userId": "",
  "productId": "",
  "orderId": ""
}
```

### Automated Tests
Use the provided test scripts to validate API responses.

---

## API Versioning Strategy

### Current Version: v1

### Deprecation Policy
- New versions announced 6 months in advance
- Old versions supported for 12 months after deprecation
- Breaking changes only in new major versions

### Version Header
```http
X-API-Version: v1
```

---

## Support and Resources

### Documentation
- API Reference: https://docs.ecommerce.com/api
- OpenAPI Spec: https://api.ecommerce.com/swagger

### Support Channels
- Email: api-support@ecommerce.com
- Developer Portal: https://developer.ecommerce.com
- Status Page: https://status.ecommerce.com

### SDKs
- .NET: `dotnet add package ECommerce.SDK`
- JavaScript: `npm install @ecommerce/sdk`
- Python: `pip install ecommerce-sdk`

---

**API Version**: v1  
**Last Updated**: November 2025  
**Maintained By**: API Team Address
```http
DELETE /api/v1/users/addresses/{addressId}
Authorization: Bearer {access_token}
```

**Response (204 No Content)**

---

## Product Catalog Service API

### Base Path: `/api/v1/products`

### Get All Products
```http
GET /api/v1/products?pageNumber=1&pageSize=20&category=electronics&minPrice=100&maxPrice=1000&sortBy=price&sortOrder=asc
Authorization: Bearer {access_token} (optional)
```

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 20, max: 100)
- `category` (optional)
- `minPrice` (optional)
- `maxPrice` (optional)
- `sortBy` (optional: name, price, createdAt, rating)
- `sortOrder` (optional: asc, desc)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "prod-001",
        "name": "Wireless Headphones",
        "slug": "wireless-headphones",
        "description": "High-quality noise-canceling headphones",
        "price": 299.99,
        "compareAtPrice": 399.99,
        "sku": "WH-001",
        "category": {
          "id": "cat-001",
          "name": "Electronics",
          "slug": "electronics"
        },
        "images": [
          {
            "url": "https://cdn.example.com/products/wh-001-1.jpg",
            "alt": "Wireless Headphones Front View",
            "isPrimary": true
          }
        ],
        "stock": 150,
        "inStock": true,
        "rating": 4.5,
        "reviewCount": 234,
        "tags": ["wireless", "bluetooth", "noise-canceling"],
        "attributes": [
          {
            "name": "Color",
            "value": "Black"
          },
          {
            "name": "Battery Life",
            "value": "30 hours"
          }
        ],
        "createdAt": "2024-01-15T00:00:00Z",
        "updatedAt": "2025-11-01T12:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 10,
    "totalCount": 195
  }
}
```

### Get Product by ID
```http
GET /api/v1/products/{productId}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "prod-001",
    "name": "Wireless Headphones",
    "slug": "wireless-headphones",
    "description": "High-quality noise-canceling headphones with 30-hour battery life...",
    "longDescription": "Detailed product description...",
    "price": 299.99,
    "compareAtPrice": 399.99,
    "sku": "WH-001",
    "category": {
      "id": "cat-001",
      "name": "Electronics",
      "slug": "electronics"
    },
    "images": [...],
    "stock": 150,
    "inStock": true,
    "rating": 4.5,
    "reviewCount": 234,
    "reviews": [
      {
        "id": "rev-001",
        "userId": "user-123",
        "userName": "John D.",
        "rating": 5,
        "title": "Excellent headphones!",
        "comment": "Best purchase I've made this year...",
        "createdAt": "2025-10-15T10:30:00Z"
      }
    ],
    "specifications": {
      "brand": "AudioTech",
      "model": "AT-WH1000",
      "weight": "250g",
      "warranty": "2 years"
    },
    "relatedProducts": ["prod-002", "prod-003"]
  }
}
```

### Search Products
```http
GET /api/v1/products/search?q=headphones&pageNumber=1&pageSize=20
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "query": "headphones",
    "totalCount": 42,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

### Create Product (Admin Only)
```http
POST /api/v1/products
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "name": "Smart Watch Pro",
  "slug": "smart-watch-pro",
  "description": "Advanced fitness tracking smartwatch",
  "longDescription": "Full detailed description...",
  "price": 399.99,
  "compareAtPrice": 499.99,
  "sku": "SW-PRO-001",
  "categoryId": "cat-002",
  "images": [
    {
      "url": "https://cdn.example.com/products/sw-001-1.jpg",
      "alt": "Smart Watch Front",
      "isPrimary": true
    }
  ],
  "stock": 100,
  "tags": ["smartwatch", "fitness", "health"],
  "attributes": [
    {
      "name": "Color",
      "value": "Silver"
    }
  ],
  "specifications": {
    "brand": "TechWear",
    "waterResistance": "5ATM"
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "prod-100",
    "name": "Smart Watch Pro",
    "slug": "smart-watch-pro",
    ...
  },
  "message": "Product created successfully"
}
```

### Update Product (Admin Only)
```http
PUT /api/v1/products/{productId}
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "name": "Smart Watch Pro - Updated",
  "price": 379.99,
  "stock": 120
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {...},
  "message": "Product updated successfully"
}
```

### Delete Product (Admin Only)
```http
DELETE /api/v1/products/{productId}
Authorization: Bearer {admin_access_token}
```

**Response (204 No Content)**

### Get Categories
```http
GET /api/v1/categories
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "cat-001",
      "name": "Electronics",
      "slug": "electronics",
      "description": "Electronic devices and accessories",
      "image": "https://cdn.example.com/categories/electronics.jpg",
      "productCount": 245,
      "parentId": null,
      "children": [
        {
          "id": "cat-001-01",
          "name": "Audio",
          "slug": "electronics-audio",
          "productCount": 87
        }
      ]
    }
  ]
}
```

### Product Reviews

#### Add Review
```http
POST /api/v1/products/{productId}/reviews
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "rating": 5,
  "title": "Amazing product!",
  "comment": "I love this product. Highly recommended!",
  "orderId": "order-123"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "rev-100",
    "productId": "prod-001",
    "userId": "user-123",
    "rating": 5,
    "title": "Amazing product!",
    "comment": "I love this product. Highly recommended!",
    "createdAt": "2025-11-02T10:30:00Z"
  }
}
```

---

## Shopping Cart Service API

### Base Path: `/api/v1/cart`

### Get Cart
```http
GET /api/v1/cart
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": "user-123",
    "items": [
      {
        "id": "cart-item-001",
        "productId": "prod-001",
        "productName": "Wireless Headphones",
        "productImage": "https://cdn.example.com/products/wh-001-1.jpg",
        "sku": "WH-001",
        "price": 299.99,
        "quantity": 2,
        "subtotal": 599.98,
        "addedAt": "2025-11-01T14:30:00Z"
      },
      {
        "id": "cart-item-002",
        "productId": "prod-002",
        "productName": "Smart Watch",
        "productImage": "https://cdn.example.com/products/sw-001-1.jpg",
        "sku": "SW-001",
        "price": 399.99,
        "quantity": 1,
        "subtotal": 399.99,
        "addedAt": "2025-11-02T09:15:00Z"
      }
    ],
    "itemCount": 3,
    "subtotal": 999.97,
    "discount": 0,
    "tax": 79.99,
    "total": 1079.96,
    "couponCode": null,
    "updatedAt": "2025-11-02T09:15:00Z"
  }
}
```

### Add Item to Cart
```http
POST /api/v1/cart/items
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "productId": "prod-003",
  "quantity": 1
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "cart-item-003",
    "productId": "prod-003",
    "productName": "Laptop Stand",
    "price": 49.99,
    "quantity": 1,
    "subtotal": 49.99
  },
  "message": "Item added to cart"
}
```

### Update Cart Item
```http
PUT /api/v1/cart/items/{itemId}
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "quantity": 3
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "cart-item-001",
    "quantity": 3,
    "subtotal": 899.97
  },
  "message": "Cart item updated"
}
```

### Remove Item from Cart
```http
DELETE /api/v1/cart/items/{itemId}
Authorization: Bearer {access_token}
```

**Response (204 No Content)**

### Clear Cart
```http
DELETE /api/v1/cart
Authorization: Bearer {access_token}
```

**Response (204 No Content)**

### Apply Coupon
```http
POST /api/v1/cart/apply-coupon
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "couponCode": "SAVE20"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "couponCode": "SAVE20",
    "discountType": "percentage",
    "discountValue": 20,
    "discountAmount": 199.99,
    "subtotal": 999.97,
    "discount": 199.99,
    "tax": 64.00,
    "total": 863.98
  },
  "message": "Coupon applied successfully"
}
```

### Remove Coupon
```http
DELETE /api/v1/cart/coupon
Authorization: Bearer {access_token}
```

**Response (200 OK)**

---

## Order Service API

### Base Path: `/api/v1/orders`

### Create Order
```http
POST /api/v1/orders
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "shippingAddressId": "addr-001",
  "billingAddressId": "addr-001",
  "paymentMethodId": "pm-001",
  "couponCode": "SAVE20",
  "notes": "Please deliver between 2-5 PM"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "order-100",
    "orderNumber": "ORD-2025-000100",
    "userId": "user-123",
    "status": "pending",
    "items": [
      {
        "id": "order-item-001",
        "productId": "prod-001",
        "productName": "Wireless Headphones",
        "sku": "WH-001",
        "price": 299.99,
        "quantity": 2,
        "subtotal": 599.98
      }
    ],
    "subtotal": 999.97,
    "discount": 199.99,
    "tax": 64.00,
    "shippingCost": 0,
    "total": 863.98,
    "shippingAddress": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    },
    "billingAddress": {...},
    "paymentMethod": "Credit Card",
    "couponCode": "SAVE20",
    "notes": "Please deliver between 2-5 PM",
    "createdAt": "2025-11-02T10:30:00Z"
  },
  "message": "Order created successfully"
}
```

### Get Order by ID
```http
GET /api/v1/orders/{orderId}
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "order-100",
    "orderNumber": "ORD-2025-000100",
    "status": "shipped",
    "statusHistory": [
      {
        "status": "pending",
        "timestamp": "2025-11-02T10:30:00Z",
        "note": "Order created"
      },
      {
        "status": "paid",
        "timestamp": "2025-11-02T10:35:00Z",
        "note": "Payment confirmed"
      },
      {
        "status": "processing",
        "timestamp": "2025-11-02T11:00:00Z",
        "note": "Order is being prepared"
      },
      {
        "status": "shipped",
        "timestamp": "2025-11-02T16:00:00Z",
        "note": "Order shipped via FedEx",
        "trackingNumber": "123456789"
      }
    ],
    "items": [...],
    "subtotal": 999.97,
    "discount": 199.99,
    "tax": 64.00,
    "shippingCost": 0,
    "total": 863.98,
    "shippingAddress": {...},
    "trackingNumber": "123456789",
    "carrier": "FedEx",
    "estimatedDelivery": "2025-11-05T00:00:00Z",
    "createdAt": "2025-11-02T10:30:00Z",
    "updatedAt": "2025-11-02T16:00:00Z"
  }
}
```

### Get User Orders
```http
GET /api/v1/orders?pageNumber=1&pageSize=10&status=shipped
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `status` (optional: pending, paid, processing, shipped, delivered, cancelled)
- `startDate` (optional)
- `endDate` (optional)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "order-100",
        "orderNumber": "ORD-2025-000100",
        "status": "shipped",
        "itemCount": 3,
        "total": 863.98,
        "createdAt": "2025-11-02T10:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 47
  }
}
```

### Cancel Order
```http
POST /api/v1/orders/{orderId}/cancel
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "reason": "Changed my mind"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "order-100",
    "status": "cancelled",
    "cancelledAt": "2025-11-02T11:00:00Z",
    "cancelReason": "Changed my mind"
  },
  "message": "Order cancelled successfully"
}
```

### Update Order Status (Admin Only)
```http
PUT /api/v1/orders/{orderId}/status
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "status": "shipped",
  "note": "Order shipped via FedEx",
  "trackingNumber": "123456789",
  "carrier": "FedEx"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "order-100",
    "status": "shipped",
    "trackingNumber": "123456789",
    "carrier": "FedEx"
  },
  "message": "Order status updated"
}
```

### Get Invoice
```http
GET /api/v1/orders/{orderId}/invoice
Authorization: Bearer {access_token}
Accept: application/pdf
```

**Response (200 OK):**
Returns PDF file

---

## Payment Service API

### Base Path: `/api/v1/payments`

### Process Payment
```http
POST /api/v1/payments/process
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "orderId": "order-100",
  "paymentMethodId": "pm-001",
  "amount": 863.98,
  "currency": "USD",
  "paymentGateway": "stripe",
  "returnUrl": "https://example.com/payment/success",
  "cancelUrl": "https://example.com/payment/cancel"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "paymentId": "pay-123",
    "orderId": "order-100",
    "status": "succeeded",
    "amount": 863.98,
    "currency": "USD",
    "paymentMethod": "card",
    "last4": "4242",
    "brand": "visa",
    "transactionId": "txn_abc123",
    "receipt": "https://stripe.com/receipt/xyz",
    "createdAt": "2025-11-02T10:35:00Z"
  },
  "message": "Payment processed successfully"
}
```

### Get Payment Details
```http
GET /api/v1/payments/{paymentId}
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "pay-123",
    "orderId": "order-100",
    "status": "succeeded",
    "amount": 863.98,
    "currency": "USD",
    "paymentMethod": "card",
    "last4": "4242",
    "brand": "visa",
    "transactionId": "txn_abc123",
    "receipt": "https://stripe.com/receipt/xyz",
    "refunds": [],
    "createdAt": "2025-11-02T10:35:00Z"
  }
}
```

### Refund Payment
```http
POST /api/v1/payments/{paymentId}/refund
Authorization: Bearer {admin_access_token}
Content-Type: application/json

{
  "amount": 863.98,
  "reason": "Customer request"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "refundId": "ref-123",
    "paymentId": "pay-123",
    "amount": 863.98,
    "status": "succeeded",
    "reason": "Customer request",
    "createdAt": "2025-11-02T12:00:00Z"
  },
  "message": "Refund processed successfully"
}
```

### Add Payment Method
```http
POST /api/v1/payments/methods
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "type": "card",
  "cardToken": "tok_visa",
  "isDefault": true
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "pm-002",
    "type": "card",
    "last4": "4242",
    "brand": "visa",
    "expiryMonth": 12,
    "expiryYear": 2026,
    "isDefault": true,
    "createdAt": "2025-11-02T10:30:00Z"
  }
}
```

### Get Payment Methods
```http
GET /api/v1/payments/methods
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "pm-001",
      "type": "card",
      "last4": "4242",
      "brand": "visa",
      "expiryMonth": 12,
      "expiryYear": 2025,
      "isDefault": true
    }
  ]
}
```

### Delete