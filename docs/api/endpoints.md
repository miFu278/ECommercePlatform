# API Endpoints Reference

Tài liệu này liệt kê tất cả các endpoint có sẵn trong E-Commerce Platform.

---

## Authentication Service

**Base Path:** `/api/auth`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Đăng ký tài khoản mới | No |
| POST | `/api/auth/login` | Đăng nhập | No |
| POST | `/api/auth/refresh-token` | Làm mới access token | No |
| POST | `/api/auth/logout` | Đăng xuất | Yes |
| GET | `/api/auth/verify-email` | Xác thực email | No |
| POST | `/api/auth/forgot-password` | Yêu cầu đặt lại mật khẩu | No |
| POST | `/api/auth/reset-password` | Đặt lại mật khẩu | No |
| POST | `/api/auth/resend-verification-email` | Gửi lại email xác thực | No |

---

## User Service

**Base Path:** `/api/user`

### User Profile

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user/profile` | Lấy thông tin profile | Yes |
| PUT | `/api/user/profile` | Cập nhật profile | Yes |
| POST | `/api/user/change-password` | Đổi mật khẩu | Yes |
| DELETE | `/api/user/account` | Xóa tài khoản | Yes |
| GET | `/api/user/{id}` | Lấy user theo ID (Admin) | Yes (Admin) |
| GET | `/api/user` | Lấy danh sách users (Admin) | Yes (Admin) |

### Address Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user/address` | Lấy tất cả địa chỉ | Yes |
| GET | `/api/user/address/{id}` | Lấy địa chỉ theo ID | Yes |
| POST | `/api/user/address` | Tạo địa chỉ mới | Yes |
| PUT | `/api/user/address/{id}` | Cập nhật địa chỉ | Yes |
| DELETE | `/api/user/address/{id}` | Xóa địa chỉ | Yes |
| PUT | `/api/user/address/{id}/set-default` | Đặt địa chỉ mặc định | Yes |

### Session Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user/session` | Lấy tất cả sessions | Yes |
| DELETE | `/api/user/session/{sessionId}` | Thu hồi session | Yes |
| POST | `/api/user/session/revoke-all` | Thu hồi tất cả sessions | Yes |

---

## Product Service

**Base Path:** `/api/products`

### Products

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/products` | Lấy danh sách sản phẩm (có filter) | No |
| GET | `/api/products/{id}` | Lấy sản phẩm theo ID | No |
| GET | `/api/products/slug/{slug}` | Lấy sản phẩm theo slug | No |
| GET | `/api/products/search` | Tìm kiếm sản phẩm | No |
| GET | `/api/products/featured` | Lấy sản phẩm nổi bật | No |
| GET | `/api/products/{id}/related` | Lấy sản phẩm liên quan | No |
| GET | `/api/products/category/{categoryId}` | Lấy sản phẩm theo danh mục | No |
| POST | `/api/products` | Tạo sản phẩm mới | Yes (Admin) |
| PUT | `/api/products/{id}` | Cập nhật sản phẩm | Yes (Admin) |
| PATCH | `/api/products/{id}/stock` | Cập nhật số lượng tồn kho | Yes (Admin) |
| DELETE | `/api/products/{id}` | Xóa sản phẩm | Yes (Admin) |
| POST | `/api/products/{id}/images` | Upload ảnh sản phẩm | Yes (Admin) |
| DELETE | `/api/products/{id}/images` | Xóa ảnh sản phẩm | Yes (Admin) |
| POST | `/api/products/seed` | Seed dữ liệu mẫu | No |

### Categories

**Base Path:** `/api/categories`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/categories` | Lấy tất cả danh mục | No |
| GET | `/api/categories/root` | Lấy danh mục gốc | No |
| GET | `/api/categories/{id}` | Lấy danh mục theo ID | No |
| GET | `/api/categories/{parentId}/children` | Lấy danh mục con | No |
| POST | `/api/categories` | Tạo danh mục mới | Yes (Admin) |
| PUT | `/api/categories/{id}` | Cập nhật danh mục | Yes (Admin) |
| DELETE | `/api/categories/{id}` | Xóa danh mục | Yes (Admin) |
| POST | `/api/categories/seed` | Seed dữ liệu mẫu | No |

### Tags

**Base Path:** `/api/tags`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/tags` | Lấy tất cả tags | No |
| GET | `/api/tags/{id}` | Lấy tag theo ID | No |
| GET | `/api/tags/name/{name}` | Lấy tag theo tên | No |
| POST | `/api/tags` | Tạo tag mới | Yes (Admin) |
| PUT | `/api/tags/{id}` | Cập nhật tag | Yes (Admin) |
| DELETE | `/api/tags/{id}` | Xóa tag | Yes (Admin) |

### Images

**Base Path:** `/api/images`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/images/upload` | Upload ảnh | No |
| DELETE | `/api/images` | Xóa ảnh | No |

---

## Shopping Cart Service

**Base Path:** `/api/cart`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/cart` | Lấy giỏ hàng | Yes |
| POST | `/api/cart/items` | Thêm sản phẩm vào giỏ | Yes |
| PUT | `/api/cart/items/{productId}` | Cập nhật số lượng | Yes |
| DELETE | `/api/cart/items/{productId}` | Xóa sản phẩm khỏi giỏ | Yes |
| DELETE | `/api/cart` | Xóa toàn bộ giỏ hàng | Yes |
| POST | `/api/cart/merge` | Gộp giỏ hàng | Yes |

---

## Order Service

**Base Path:** `/api/orders`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/orders` | Tạo đơn hàng mới | Yes |
| GET | `/api/orders/{id}` | Lấy đơn hàng theo ID | Yes |
| GET | `/api/orders/number/{orderNumber}` | Lấy đơn hàng theo số | Yes |
| GET | `/api/orders/my-orders` | Lấy đơn hàng của user | Yes |
| GET | `/api/orders` | Lấy tất cả đơn hàng (Admin) | Yes (Admin) |
| GET | `/api/orders/status/{status}` | Lấy đơn hàng theo trạng thái (Admin) | Yes (Admin) |
| PATCH | `/api/orders/{id}/status` | Cập nhật trạng thái (Admin) | Yes (Admin) |
| POST | `/api/orders/{id}/cancel` | Hủy đơn hàng | Yes |
| GET | `/api/orders/statistics` | Thống kê đơn hàng (Admin) | Yes (Admin) |
| GET | `/api/orders/dashboard` | Dashboard data (Admin) | Yes (Admin) |

---

## Payment Service

**Base Path:** `/api/payments`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/payments` | Tạo thanh toán | Yes |
| GET | `/api/payments/{id}` | Lấy thanh toán theo ID | Yes |
| GET | `/api/payments/order/{orderId}` | Lấy thanh toán theo order ID | Yes |
| GET | `/api/payments/my-payments` | Lấy thanh toán của user | Yes |
| POST | `/api/payments/{id}/refund` | Hoàn tiền (Admin) | Yes (Admin) |
| POST | `/api/payments/{id}/cancel` | Hủy thanh toán | Yes |
| POST | `/api/payments/webhook` | PayOS webhook | No |
| GET | `/api/payments/return` | PayOS return URL | No |

---

## Notification Service

**Note:** Notification service không có public API endpoints. Service này được gọi nội bộ thông qua message queue (RabbitMQ) để gửi email và SMS notifications.

### Internal Events (RabbitMQ)

- `user.registered` - Gửi email xác thực khi user đăng ký
- `user.email-verification` - Gửi email xác thực
- `user.password-reset` - Gửi email đặt lại mật khẩu
- `order.created` - Gửi email xác nhận đơn hàng
- `order.status-changed` - Gửi email khi trạng thái đơn hàng thay đổi
- `payment.completed` - Gửi email xác nhận thanh toán

---

## Tổng kết

### Tổng số Endpoints: 80+

- **Authentication:** 8 endpoints
- **User Service:** 18 endpoints
- **Product Service:** 32 endpoints
- **Shopping Cart:** 6 endpoints
- **Order Service:** 10 endpoints
- **Payment Service:** 8 endpoints
- **Notification Service:** Internal only (RabbitMQ events)

### Authentication Requirements

- **Public endpoints:** 42 endpoints (không cần auth)
- **User endpoints:** 28 endpoints (cần auth)
- **Admin endpoints:** 18 endpoints (cần admin role)

---

**Last Updated:** December 2, 2025
