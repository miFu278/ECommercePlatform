# E-Commerce Platform - API Documentation

## Mục lục
1. [Tổng quan API](#tổng-quan-api)
2. [Authentication Service](#authentication-service)
3. [User Service](#user-service)
4. [Product Service](#product-service)
5. [Shopping Cart Service](#shopping-cart-service)
6. [Order Service](#order-service)
7. [Payment Service](#payment-service)
8. [Xử lý lỗi](#xử-lý-lỗi)

---

## Tổng quan API

### Base URL
```
Development: http://localhost:5000
Production:  https://api.ecommerce.com
```

### Common Headers
```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer {access_token}
```

### Standard Response Format

**Success Response:**
```json
{
  "data": { },
  "message": "Success"
}
```

**Error Response:**
```json
{
  "message": "Error message"
}
```

---

## Authentication Service

### Base Path: `/api/auth`

### 1. Register User
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (200 OK):**
```json
{
  "message": "Registration successful. Please check your email to verify your account.",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "username": "johndoe"
  }
}
```

**Rate Limit:** Enabled

---

### 2. Login
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "username": "johndoe",
    "role": "Customer"
  }
}
```

**Rate Limit:** Enabled

---

### 3. Refresh Token
**Endpoint:** `POST /api/auth/refresh-token`

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "new_access_token",
  "refreshToken": "new_refresh_token",
  "expiresIn": 3600
}
```

---

### 4. Logout
**Endpoint:** `POST /api/auth/logout`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

---

### 5. Verify Email
**Endpoint:** `GET /api/auth/verify-email?token={token}`

**Response (200 OK):**
```json
{
  "message": "Email verified successfully"
}
```

---

### 6. Forgot Password
**Endpoint:** `POST /api/auth/forgot-password`

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "message": "If the email exists, a password reset link has been sent"
}
```

**Rate Limit:** Enabled

---

### 7. Reset Password
**Endpoint:** `POST /api/auth/reset-password`

**Request Body:**
```json
{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "NewPass789!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password reset successfully"
}
```

**Rate Limit:** Enabled

---

### 8. Resend Verification Email
**Endpoint:** `POST /api/auth/resend-verification-email`

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "message": "If the email exists and is not verified, a verification link has been sent"
}
```

---

## User Service

### Base Path: `/api/user`

### 1. Get User Profile
**Endpoint:** `GET /api/user/profile`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "role": "Customer",
  "isEmailVerified": true,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

### 2. Update User Profile
**Endpoint:** `PUT /api/user/profile`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "username": "johndoe_updated",
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890"
}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "username": "johndoe_updated",
  "firstName": "John",
  "lastName": "Smith"
}
```

---

### 3. Change Password
**Endpoint:** `POST /api/user/change-password`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password changed successfully. Please login again."
}
```

---

### 4. Delete Account
**Endpoint:** `DELETE /api/user/account`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "password": "CurrentPass123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Account deleted successfully"
}
```

---

### 5. Get User by ID (Admin Only)
**Endpoint:** `GET /api/user/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

**Response (200 OK):**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Customer"
}
```

---

### 6. Get All Users (Admin Only)
**Endpoint:** `GET /api/user?pageNumber=1&pageSize=10`  
**Authorization:** Bearer Token Required (Admin Role)

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "guid",
      "email": "user@example.com",
      "username": "johndoe"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100
}
```

---

## Address Management

### Base Path: `/api/user/address`

### 1. Get All Addresses
**Endpoint:** `GET /api/user/address`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
[
  {
    "id": "guid",
    "recipientName": "John Doe",
    "phoneNumber": "+1234567890",
    "addressLine1": "123 Main St",
    "addressLine2": "Apt 4B",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA",
    "isDefault": true
  }
]
```

---

### 2. Get Address by ID
**Endpoint:** `GET /api/user/address/{id}`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "recipientName": "John Doe",
  "phoneNumber": "+1234567890",
  "addressLine1": "123 Main St",
  "city": "New York",
  "isDefault": true
}
```

---

### 3. Create Address
**Endpoint:** `POST /api/user/address`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "recipientName": "John Doe",
  "phoneNumber": "+1234567890",
  "addressLine1": "123 Main St",
  "addressLine2": "Apt 4B",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "isDefault": false
}
```

**Response (201 Created):**
```json
{
  "id": "guid",
  "recipientName": "John Doe",
  "addressLine1": "123 Main St",
  "isDefault": false
}
```

---

### 4. Update Address
**Endpoint:** `PUT /api/user/address/{id}`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "recipientName": "John Smith",
  "phoneNumber": "+1234567890",
  "addressLine1": "456 Oak Ave",
  "city": "Los Angeles",
  "state": "CA",
  "postalCode": "90001",
  "country": "USA"
}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "recipientName": "John Smith",
  "addressLine1": "456 Oak Ave"
}
```

---

### 5. Delete Address
**Endpoint:** `DELETE /api/user/address/{id}`  
**Authorization:** Bearer Token Required

**Response (204 No Content)**

---

### 6. Set Default Address
**Endpoint:** `PUT /api/user/address/{id}/set-default`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "isDefault": true
}
```

---

## Session Management

### Base Path: `/api/user/session`

### 1. Get All Sessions
**Endpoint:** `GET /api/user/session?currentRefreshToken={token}`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
[
  {
    "id": "guid",
    "deviceInfo": "Chrome on Windows",
    "ipAddress": "192.168.1.1",
    "location": "New York, USA",
    "isCurrent": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "lastActivityAt": "2024-01-02T10:30:00Z"
  }
]
```

---

### 2. Revoke Session
**Endpoint:** `DELETE /api/user/session/{sessionId}`  
**Authorization:** Bearer Token Required

**Response (204 No Content)**

---

### 3. Revoke All Sessions
**Endpoint:** `POST /api/user/session/revoke-all`  
**Authorization:** Bearer Token Required

**Request Body (Optional):**
```json
{
  "exceptRefreshToken": "current_refresh_token"
}
```

**Response (200 OK):**
```json
{
  "message": "All other sessions have been revoked"
}
```

---

## Product Service

### Base Path: `/api/products`

### 1. Get All Products (Search & Filter)
**Endpoint:** `GET /api/products`

**Query Parameters:**
- `keyword` - Tìm kiếm theo tên
- `categoryId` - Lọc theo danh mục
- `minPrice` - Giá tối thiểu
- `maxPrice` - Giá tối đa
- `tags` - Lọc theo tags
- `inStock` - Chỉ hiển thị sản phẩm còn hàng
- `sortBy` - Sắp xếp (name, price, createdAt)
- `sortDescending` - Sắp xếp giảm dần
- `pageNumber` - Trang (default: 1)
- `pageSize` - Số items/trang (default: 10)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "string",
      "name": "Wireless Headphones",
      "slug": "wireless-headphones",
      "description": "High-quality headphones",
      "price": 299.99,
      "compareAtPrice": 399.99,
      "sku": "WH-001",
      "stockQuantity": 150,
      "imageUrl": "https://cdn.example.com/image.jpg",
      "categoryName": "Electronics",
      "tags": ["wireless", "bluetooth"],
      "isFeatured": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10
}
```

---

### 2. Get Product by ID
**Endpoint:** `GET /api/products/{id}`

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "Wireless Headphones",
  "slug": "wireless-headphones",
  "description": "High-quality noise-canceling headphones",
  "price": 299.99,
  "compareAtPrice": 399.99,
  "sku": "WH-001",
  "stockQuantity": 150,
  "images": [
    "https://cdn.example.com/image1.jpg",
    "https://cdn.example.com/image2.jpg"
  ],
  "category": {
    "id": "string",
    "name": "Electronics"
  },
  "tags": ["wireless", "bluetooth", "noise-canceling"],
  "isFeatured": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

---

### 3. Get Product by Slug
**Endpoint:** `GET /api/products/slug/{slug}`

**Response (200 OK):** Same as Get Product by ID

---

### 4. Search Products
**Endpoint:** `GET /api/products/search?query={keyword}`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Wireless Headphones",
    "slug": "wireless-headphones",
    "price": 299.99,
    "imageUrl": "https://cdn.example.com/image.jpg"
  }
]
```

---

### 5. Get Featured Products
**Endpoint:** `GET /api/products/featured?limit=10`

**Query Parameters:**
- `limit` - Số lượng sản phẩm (1-50, default: 10)

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Wireless Headphones",
    "price": 299.99,
    "imageUrl": "https://cdn.example.com/image.jpg"
  }
]
```

---

### 6. Get Related Products
**Endpoint:** `GET /api/products/{id}/related?limit=5`

**Query Parameters:**
- `limit` - Số lượng sản phẩm (1-20, default: 5)

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Similar Product",
    "price": 249.99,
    "imageUrl": "https://cdn.example.com/image.jpg"
  }
]
```

---

### 7. Get Products by Category
**Endpoint:** `GET /api/products/category/{categoryId}`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Product Name",
    "price": 299.99
  }
]
```

---

### 8. Create Product (Admin)
**Endpoint:** `POST /api/products`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "Smart Watch Pro",
  "slug": "smart-watch-pro",
  "description": "Advanced fitness tracking smartwatch",
  "price": 399.99,
  "compareAtPrice": 499.99,
  "sku": "SW-PRO-001",
  "categoryId": "string",
  "stockQuantity": 100,
  "images": [
    "https://cdn.example.com/image.jpg"
  ],
  "tags": ["smartwatch", "fitness"],
  "isFeatured": false
}
```

**Response (201 Created):**
```json
{
  "id": "string",
  "name": "Smart Watch Pro",
  "slug": "smart-watch-pro",
  "price": 399.99
}
```

---

### 9. Update Product (Admin)
**Endpoint:** `PUT /api/products/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "Smart Watch Pro - Updated",
  "description": "Updated description",
  "price": 379.99,
  "stockQuantity": 120
}
```

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "Smart Watch Pro - Updated",
  "price": 379.99
}
```

---

### 10. Update Stock (Admin)
**Endpoint:** `PATCH /api/products/{id}/stock`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "quantity": 200
}
```

**Response (200 OK):**
```json
{
  "message": "Stock updated successfully",
  "quantity": 200
}
```

---

### 11. Delete Product (Admin)
**Endpoint:** `DELETE /api/products/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Response (204 No Content)**

---

### 12. Upload Product Image (Admin)
**Endpoint:** `POST /api/products/{id}/images`  
**Authorization:** Bearer Token Required (Admin)  
**Content-Type:** multipart/form-data

**Form Data:**
- `file` - Image file

**Response (200 OK):**
```json
{
  "message": "Image uploaded successfully",
  "imageUrl": "https://cdn.example.com/image.jpg"
}
```

---

### 13. Delete Product Image (Admin)
**Endpoint:** `DELETE /api/products/{id}/images?imageUrl={url}`  
**Authorization:** Bearer Token Required (Admin)

**Response (200 OK):**
```json
{
  "message": "Image deleted successfully"
}
```

---

### 14. Seed Products (Development)
**Endpoint:** `POST /api/products/seed`

**Response (200 OK):**
```json
{
  "message": "Products seeded successfully"
}
```

---

## Categories

### Base Path: `/api/categories`

### 1. Get All Categories
**Endpoint:** `GET /api/categories`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Electronics",
    "slug": "electronics",
    "description": "Electronic devices",
    "parentId": null,
    "imageUrl": "https://cdn.example.com/category.jpg",
    "displayOrder": 1,
    "isActive": true
  }
]
```

---

### 2. Get Root Categories
**Endpoint:** `GET /api/categories/root`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Electronics",
    "slug": "electronics"
  }
]
```

---

### 3. Get Category by ID
**Endpoint:** `GET /api/categories/{id}`

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "Electronics",
  "slug": "electronics",
  "description": "Electronic devices",
  "parentId": null
}
```

---

### 4. Get Child Categories
**Endpoint:** `GET /api/categories/{parentId}/children`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "Audio",
    "slug": "audio",
    "parentId": "parent-id"
  }
]
```

---

### 5. Create Category (Admin)
**Endpoint:** `POST /api/categories`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "Smart Home",
  "slug": "smart-home",
  "description": "Smart home devices",
  "parentId": null,
  "imageUrl": "https://cdn.example.com/category.jpg",
  "displayOrder": 5
}
```

**Response (201 Created):**
```json
{
  "id": "string",
  "name": "Smart Home",
  "slug": "smart-home"
}
```

---

### 6. Update Category (Admin)
**Endpoint:** `PUT /api/categories/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "Smart Home - Updated",
  "description": "Updated description"
}
```

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "Smart Home - Updated"
}
```

---

### 7. Delete Category (Admin)
**Endpoint:** `DELETE /api/categories/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Response (204 No Content)**

---

### 8. Seed Categories (Development)
**Endpoint:** `POST /api/categories/seed`

**Response (200 OK):**
```json
{
  "message": "Categories seeded successfully"
}
```

---

## Tags

### Base Path: `/api/tags`

### 1. Get All Tags
**Endpoint:** `GET /api/tags`

**Response (200 OK):**
```json
[
  {
    "id": "string",
    "name": "wireless",
    "slug": "wireless",
    "productCount": 45
  }
]
```

---

### 2. Get Tag by ID
**Endpoint:** `GET /api/tags/{id}`

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "wireless",
  "slug": "wireless"
}
```

---

### 3. Get Tag by Name
**Endpoint:** `GET /api/tags/name/{name}`

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "wireless",
  "slug": "wireless"
}
```

---

### 4. Create Tag (Admin)
**Endpoint:** `POST /api/tags`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "eco-friendly",
  "slug": "eco-friendly"
}
```

**Response (201 Created):**
```json
{
  "id": "string",
  "name": "eco-friendly",
  "slug": "eco-friendly"
}
```

---

### 5. Update Tag (Admin)
**Endpoint:** `PUT /api/tags/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "name": "eco-friendly-updated",
  "slug": "eco-friendly-updated"
}
```

**Response (200 OK):**
```json
{
  "id": "string",
  "name": "eco-friendly-updated"
}
```

---

### 6. Delete Tag (Admin)
**Endpoint:** `DELETE /api/tags/{id}`  
**Authorization:** Bearer Token Required (Admin)

**Response (204 No Content)**

---

## Images

### Base Path: `/api/images`

### 1. Upload Image
**Endpoint:** `POST /api/images/upload?folder=products`  
**Content-Type:** multipart/form-data

**Form Data:**
- `file` - Image file

**Query Parameters:**
- `folder` - Folder name (default: "products")

**Response (200 OK):**
```json
{
  "url": "https://cdn.example.com/products/image.jpg",
  "message": "Upload successful"
}
```

---

### 2. Delete Image
**Endpoint:** `DELETE /api/images?publicId={publicId}`

**Query Parameters:**
- `publicId` - Public ID of the image

**Response (200 OK):**
```json
{
  "message": "Image deleted successfully"
}
```

---

## Shopping Cart Service

### Base Path: `/api/cart`

### 1. Get Cart
**Endpoint:** `GET /api/cart`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "productName": "Wireless Headphones",
      "productImage": "https://cdn.example.com/image.jpg",
      "price": 299.99,
      "quantity": 2,
      "subtotal": 599.98
    }
  ],
  "totalItems": 2,
  "totalPrice": 599.98,
  "updatedAt": "2024-01-02T10:30:00Z"
}
```

---

### 2. Add Item to Cart
**Endpoint:** `POST /api/cart/items`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "productId": "guid",
  "quantity": 1
}
```

**Response (200 OK):**
```json
{
  "userId": "guid",
  "items": [...],
  "totalItems": 3,
  "totalPrice": 899.97
}
```

---

### 3. Update Cart Item
**Endpoint:** `PUT /api/cart/items/{productId}`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "quantity": 3
}
```

**Response (200 OK):**
```json
{
  "userId": "guid",
  "items": [...],
  "totalItems": 4,
  "totalPrice": 1199.96
}
```

---

### 4. Remove Item from Cart
**Endpoint:** `DELETE /api/cart/items/{productId}`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "userId": "guid",
  "items": [...],
  "totalItems": 1,
  "totalPrice": 299.99
}
```

---

### 5. Clear Cart
**Endpoint:** `DELETE /api/cart`  
**Authorization:** Bearer Token Required

**Response (204 No Content)**

---

### 6. Merge Carts
**Endpoint:** `POST /api/cart/merge`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "anonymousUserId": "guid"
}
```

**Response (200 OK):**
```json
{
  "merged": true
}
```

---

## Order Service

### Base Path: `/api/orders`

### 1. Create Order
**Endpoint:** `POST /api/orders`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "shippingAddressId": "guid",
  "paymentMethod": 0,
  "notes": "Please deliver between 2-5 PM"
}
```

**PaymentMethod Enum:**
- `0` - COD (Cash on Delivery)
- `1` - Banking (Online Payment)

**Response (201 Created):**
```json
{
  "id": "guid",
  "orderNumber": "ORD-2025-000100",
  "status": "Pending",
  "totalAmount": 863.98,
  "paymentUrl": "https://payment.provider.com/checkout/xyz"
}
```

---

### 2. Get Order by ID
**Endpoint:** `GET /api/orders/{id}`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "orderNumber": "ORD-2025-000100",
  "userId": "guid",
  "status": "Shipped",
  "items": [
    {
      "productId": "guid",
      "productName": "Wireless Headphones",
      "sku": "WH-001",
      "price": 299.99,
      "quantity": 2,
      "subtotal": 599.98
    }
  ],
  "subtotal": 999.97,
  "discount": 0,
  "tax": 79.99,
  "shippingCost": 0,
  "totalAmount": 1079.96,
  "shippingAddress": {
    "recipientName": "John Doe",
    "phoneNumber": "+1234567890",
    "addressLine1": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "paymentMethod": "Banking",
  "paymentStatus": "Paid",
  "notes": "Please deliver between 2-5 PM",
  "createdAt": "2025-11-02T10:30:00Z",
  "updatedAt": "2025-11-02T16:00:00Z"
}
```

---

### 3. Get Order by Order Number
**Endpoint:** `GET /api/orders/number/{orderNumber}`  
**Authorization:** Bearer Token Required

**Response (200 OK):** Same as Get Order by ID

---

### 4. Get My Orders
**Endpoint:** `GET /api/orders/my-orders?page=1&pageSize=10`  
**Authorization:** Bearer Token Required

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "guid",
      "orderNumber": "ORD-2025-000100",
      "status": "Shipped",
      "totalAmount": 863.98,
      "createdAt": "2025-11-02T10:30:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 47,
  "totalPages": 5
}
```

---

### 5. Get All Orders (Admin)
**Endpoint:** `GET /api/orders?page=1&pageSize=10`  
**Authorization:** Bearer Token Required (Admin)

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)

**Response (200 OK):**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 500,
  "totalPages": 50
}
```

---

### 6. Get Orders by Status (Admin)
**Endpoint:** `GET /api/orders/status/{status}?page=1&pageSize=10`  
**Authorization:** Bearer Token Required (Admin)

**Status Enum:**
- `Pending` - 0
- `Paid` - 1
- `Processing` - 2
- `Shipped` - 3
- `Delivered` - 4
- `Cancelled` - 5

**Response (200 OK):**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50
}
```

---

### 7. Update Order Status (Admin)
**Endpoint:** `PATCH /api/orders/{id}/status`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "status": 3,
  "notes": "Order shipped via FedEx"
}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "orderNumber": "ORD-2025-000100",
  "status": "Shipped",
  "updatedAt": "2025-11-02T16:00:00Z"
}
```

---

### 8. Cancel Order
**Endpoint:** `POST /api/orders/{id}/cancel`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "reason": "Changed my mind"
}
```

**Response (200 OK):**
```json
{
  "message": "Order cancelled successfully"
}
```

---

### 9. Get Order Statistics (Admin)
**Endpoint:** `GET /api/orders/statistics`  
**Authorization:** Bearer Token Required (Admin)

**Response (200 OK):**
```json
{
  "totalOrders": 500,
  "pendingOrders": 45,
  "processingOrders": 30,
  "shippedOrders": 25,
  "deliveredOrders": 380,
  "cancelledOrders": 20,
  "totalRevenue": 450000.00,
  "averageOrderValue": 900.00
}
```

---

### 10. Get Dashboard Data (Admin)
**Endpoint:** `GET /api/orders/dashboard`  
**Authorization:** Bearer Token Required (Admin)

**Response (200 OK):**
```json
{
  "todayOrders": 15,
  "todayRevenue": 13500.00,
  "weekOrders": 105,
  "weekRevenue": 94500.00,
  "monthOrders": 450,
  "monthRevenue": 405000.00,
  "recentOrders": [...]
}
```

---

## Payment Service

### Base Path: `/api/payments`

### 1. Create Payment
**Endpoint:** `POST /api/payments`  
**Authorization:** Bearer Token Required

**Request Body:**
```json
{
  "orderId": "guid",
  "method": 1,
  "paymentToken": null
}
```

**Payment Method Enum:**
- `0` - COD (Cash on Delivery)
- `1` - Online (Banking/Card)

**Response (201 Created):**
```json
{
  "id": "guid",
  "paymentNumber": "PAY-2025-000100",
  "orderId": "guid",
  "userId": "guid",
  "amount": 863.98,
  "method": "Online",
  "status": "Pending",
  "errorMessage": "https://payment.provider.com/checkout/xyz",
  "providerTransactionId": "txn_abc123",
  "createdAt": "2025-11-02T10:35:00Z"
}
```

**Note:** `errorMessage` field temporarily contains PayOS checkout URL for online payments.

---

### 2. Get Payment by ID
**Endpoint:** `GET /api/payments/{id}`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "paymentNumber": "PAY-2025-000100",
  "orderId": "guid",
  "userId": "guid",
  "amount": 863.98,
  "method": "Online",
  "status": "Completed",
  "providerTransactionId": "txn_abc123",
  "createdAt": "2025-11-02T10:35:00Z",
  "completedAt": "2025-11-02T10:36:00Z"
}
```

---

### 3. Get Payment by Order ID
**Endpoint:** `GET /api/payments/order/{orderId}`  
**Authorization:** Bearer Token Required

**Response (200 OK):** Same as Get Payment by ID

---

### 4. Get My Payments
**Endpoint:** `GET /api/payments/my-payments?page=1&pageSize=10`  
**Authorization:** Bearer Token Required

**Query Parameters:**
- `page` (default: 1)
- `pageSize` (default: 10)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "guid",
      "paymentNumber": "PAY-2025-000100",
      "orderId": "guid",
      "amount": 863.98,
      "method": "Online",
      "status": "Completed",
      "createdAt": "2025-11-02T10:35:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3
}
```

---

### 5. Refund Payment (Admin)
**Endpoint:** `POST /api/payments/{id}/refund`  
**Authorization:** Bearer Token Required (Admin)

**Request Body:**
```json
{
  "amount": 863.98,
  "reason": "Customer request"
}
```

**Response (200 OK):**
```json
{
  "id": "guid",
  "paymentNumber": "PAY-2025-000100",
  "status": "Refunded",
  "refundedAmount": 863.98,
  "refundReason": "Customer request",
  "refundedAt": "2025-11-02T12:00:00Z"
}
```

---

### 6. Cancel Payment
**Endpoint:** `POST /api/payments/{id}/cancel`  
**Authorization:** Bearer Token Required

**Response (200 OK):**
```json
{
  "id": "guid",
  "paymentNumber": "PAY-2025-000100",
  "status": "Cancelled",
  "cancelledAt": "2025-11-02T11:00:00Z"
}
```

---

### 7. PayOS Webhook
**Endpoint:** `POST /api/payments/webhook`  
**Note:** This endpoint is called by PayOS payment provider

**Request Body:**
```json
{
  "code": "00",
  "desc": "Success",
  "data": {
    "orderCode": 123456789,
    "amount": 86398,
    "description": "Payment for order",
    "accountNumber": "1234567890",
    "reference": "FT123456",
    "transactionDateTime": "2025-11-02T10:36:00Z"
  },
  "signature": "signature_hash"
}
```

**Response (200 OK):**
```json
{
  "message": "Webhook processed successfully"
}
```

---

### 8. PayOS Return URL
**Endpoint:** `GET /api/payments/return`  
**Note:** User is redirected here after payment

**Query Parameters:**
- `code` - Response code
- `id` - Payment ID
- `cancel` - Cancellation flag
- `status` - Payment status
- `orderCode` - Order code

**Response:** Redirect to appropriate page
- Success: `/payment/success?orderCode={orderCode}`
- Cancelled: `/payment/cancelled?orderCode={orderCode}`
- Failed: `/payment/failed?orderCode={orderCode}&status={status}`

---

## Xử lý lỗi

### HTTP Status Codes

| Status Code | Description |
|------------|-------------|
| 200 | OK - Request succeeded |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request succeeded with no content |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Not authorized |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource conflict |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error |

---

### Error Response Format

```json
{
  "message": "Error description"
}
```

### Common Error Examples

#### Validation Error (400)
```json
{
  "message": "Email is required"
}
```

#### Authentication Error (401)
```json
{
  "message": "Invalid credentials"
}
```

#### Authorization Error (403)
```json
{
  "message": "You do not have permission to access this resource"
}
```

#### Not Found Error (404)
```json
{
  "message": "Product not found"
}
```

#### Conflict Error (409)
```json
{
  "message": "Email already exists"
}
```

#### Rate Limit Error (429)
```json
{
  "message": "Too many requests. Please try again later."
}
```

---

## Authentication & Authorization

### JWT Token Structure

**Access Token:**
- Expires in: 1 hour
- Contains: User ID, Email, Username, Role

**Refresh Token:**
- Expires in: 7 days
- Used to obtain new access token

### Using Bearer Token

Include the access token in the Authorization header:

```http
Authorization: Bearer {access_token}
```

### User Roles

- **Customer** - Regular user with basic permissions
- **Admin** - Administrator with full permissions

### Protected Endpoints

Most endpoints require authentication. Admin-only endpoints are marked with "(Admin)" in the documentation.

---

## Rate Limiting

### Rate Limits by Endpoint

| Endpoint | Limit |
|----------|-------|
| `/api/auth/register` | Rate limited |
| `/api/auth/login` | Rate limited |
| `/api/auth/forgot-password` | Rate limited |
| `/api/auth/reset-password` | Rate limited |
| Other endpoints | No specific limit |

---

## API Best Practices

### 1. Always Include Authorization Header
```http
Authorization: Bearer {access_token}
```

### 2. Handle Token Expiration
When you receive a 401 error, use the refresh token to get a new access token.

### 3. Implement Retry Logic
For failed requests, implement exponential backoff retry logic.

### 4. Validate Input on Client Side
Validate data before sending to the API to reduce errors.

### 5. Handle Errors Gracefully
Always check response status and handle errors appropriately.

---

## API Client Examples

### JavaScript/TypeScript (Axios)

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000',
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
  const response = await api.post('/api/auth/login', {
    email,
    password
  });
  localStorage.setItem('accessToken', response.data.accessToken);
  localStorage.setItem('refreshToken', response.data.refreshToken);
  return response.data;
};

// Get Products
const getProducts = async (pageNumber = 1, pageSize = 10) => {
  const response = await api.get('/api/products', {
    params: { pageNumber, pageSize }
  });
  return response.data;
};

// Create Order
const createOrder = async (orderData) => {
  const response = await api.post('/api/orders', orderData);
  return response.data;
};
```

---

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
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _accessToken = result.AccessToken;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
        
        return result;
    }

    public async Task<PagedResult<Product>> GetProductsAsync(int page = 1, int pageSize = 10)
    {
        var response = await _httpClient.GetAsync(
            $"/api/products?pageNumber={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<PagedResult<Product>>();
    }
}
```

---

### Python (Requests)

```python
import requests

class ECommerceApiClient:
    def __init__(self, base_url: str):
        self.base_url = base_url
        self.session = requests.Session()
        self.session.headers.update({'Content-Type': 'application/json'})
    
    def login(self, email: str, password: str):
        response = self.session.post(
            f'{self.base_url}/api/auth/login',
            json={'email': email, 'password': password}
        )
        response.raise_for_status()
        data = response.json()
        
        # Set token for future requests
        self.session.headers.update({
            'Authorization': f"Bearer {data['accessToken']}"
        })
        
        return data
    
    def get_products(self, page_number: int = 1, page_size: int = 10):
        response = self.session.get(
            f'{self.base_url}/api/products',
            params={'pageNumber': page_number, 'pageSize': page_size}
        )
        response.raise_for_status()
        return response.json()
    
    def create_order(self, order_data: dict):
        response = self.session.post(
            f'{self.base_url}/api/orders',
            json=order_data
        )
        response.raise_for_status()
        return response.json()
```

---

### cURL Examples

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Get Products
curl -X GET "http://localhost:5000/api/products?pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer {access_token}"

# Create Order
curl -X POST http://localhost:5000/api/orders \
  -H "Authorization: Bearer {access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "shippingAddressId": "guid",
    "paymentMethod": 0,
    "notes": "Please deliver between 2-5 PM"
  }'

# Get Cart
curl -X GET http://localhost:5000/api/cart \
  -H "Authorization: Bearer {access_token}"

# Add to Cart
curl -X POST http://localhost:5000/api/cart/items \
  -H "Authorization: Bearer {access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "guid",
    "quantity": 1
  }'
```

---

## Changelog

### Version 1.0.0 (December 2025)
- Initial API documentation
- Complete documentation for all services:
  - Authentication Service
  - User Service
  - Product Service
  - Shopping Cart Service
  - Order Service
  - Payment Service
- Added API client examples for JavaScript, C#, Python, and cURL
- Documented all endpoints with request/response examples
- Added error handling documentation
- Added rate limiting information

---

**API Version**: 1.0  
**Last Updated**: December 2, 2025  
**Maintained By**: Development Team
