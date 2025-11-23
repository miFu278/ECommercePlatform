# 🧪 Test Checklist - E-Commerce Platform

**Last Updated:** November 19, 2025  
**Project Status:** In Development

---

## 📊 Overall Progress

| Service | Total Tests | Completed | Progress |
|---------|-------------|-----------|----------|
| User Service | 47 | 0 | 0% |
| Product Service | 28 | 0 | 0% |
| Shopping Cart | 22 | 0 | 0% |
| Order Service | 25 | 0 | 0% |
| Payment Service | 18 | 0 | 0% |
| Notification | 15 | 0 | 0% |
| **TOTAL** | **155** | **0** | **0%** |

---

## 🔐 User Service (47 tests)

### Authentication (26 tests)

#### Registration
- [x] Register với email và password hợp lệ → 200 OK
- [x] Register với email đã tồn tại → 409 Conflict
- [x] Register với username đã tồn tại → 409 Conflict
- [x] Register với email không hợp lệ → 400 Bad Request
- [x] Register với password yếu (< 8 ký tự) → 400 Bad Request
- [x] Register với password không có chữ hoa → 400 Bad Request
- [x] Register với password không có số → 400 Bad Request
- [x] Register với password không có ký tự đặc biệt → 400 Bad Request
- [x] Register với firstName trống → 400 Bad Request
- [x] Register với phone number không hợp lệ → 400 Bad Request
- [x] Register với dateOfBirth trong tương lai → 400 Bad Request
- [x] Register với dateOfBirth < 13 tuổi → 400 Bad Request

#### Login
- [x] Login với credentials đúng → 200 OK + JWT token
- [x] Login với email không tồn tại → 401 Unauthorized
- [x] Login với password sai → 401 Unauthorized
- [ ] Login với email chưa verify → 403 Forbidden
- [ ] Login với account đã bị xóa (soft delete) → 404 Not Found
- [ ] Login với account bị khóa → 400 Bad Request (ACCOUNT_LOCKED)

#### Email Verification
- [ ] Verify email với token hợp lệ → 200 OK
- [ ] Verify email với token hết hạn (> 24h) → 400 Bad Request
- [ ] Verify email với token không tồn tại → 404 Not Found
- [ ] Verify email với token đã dùng → 400 Bad Request
- [ ] Resend verification email → 200 OK

#### Password Reset
- [ ] Forgot password với email hợp lệ → 200 OK + Email sent
- [ ] Forgot password với email không tồn tại → 404 Not Found
- [ ] Reset password với token hợp lệ → 200 OK
- [ ] Reset password với token hết hạn (> 1h) → 400 Bad Request
- [ ] Reset password với token đã dùng → 400 Bad Request

#### Token Management
- [ ] Refresh token với valid refresh token → 200 OK + New tokens
- [ ] Refresh token với expired refresh token → 401 Unauthorized
- [ ] Refresh token với revoked token → 401 Unauthorized
- [ ] Logout với valid token → 200 OK + Token revoked
- [ ] Logout without authentication → 401 Unauthorized

#### Rate Limiting (10 tests)
- [ ] Login: 10 attempts trong 5 phút → All pass
- [ ] Login: 11th attempt trong 5 phút → 429 Too Many Requests
- [ ] Register: 5 attempts trong 15 phút → All pass
- [ ] Register: 6th attempt trong 15 phút → 429 Too Many Requests
- [ ] Forgot password: 3 attempts trong 30 phút → All pass
- [ ] Forgot password: 4th attempt trong 30 phút → 429 Too Many Requests
- [ ] Reset password: 3 attempts trong 30 phút → All pass
- [ ] Reset password: 4th attempt trong 30 phút → 429 Too Many Requests
- [ ] Rate limit response có retryAfter field → Yes
- [ ] Rate limit reset sau window time → Can retry

### Profile Management (9 tests)

- [ ] Get profile với valid token → 200 OK + User data
- [ ] Get profile với invalid token → 401 Unauthorized
- [ ] Get profile với expired token → 401 Unauthorized
- [ ] Update profile (name, phone, DOB) → 200 OK
- [ ] Update profile với phone không hợp lệ → 400 Bad Request
- [ ] Update profile với phone đã tồn tại → 409 Conflict
- [ ] Update profile với DOB trong tương lai → 400 Bad Request
- [ ] Change password với old password đúng → 200 OK
- [ ] Change password với old password sai → 400 Bad Request

### Address Management (7 tests)

- [ ] Get all addresses của user → 200 OK + List addresses
- [ ] Get address by ID → 200 OK + Address details
- [ ] Create new address → 201 Created
- [ ] Create address với data không hợp lệ → 400 Bad Request
- [ ] Update address → 200 OK
- [ ] Set address as default → 200 OK + Other addresses not default
- [ ] Delete address → 204 No Content

### Session Management (3 tests)

- [ ] Get all active sessions → 200 OK + List sessions
- [ ] Revoke specific session → 200 OK
- [ ] Revoke all sessions except current → 200 OK

### Admin Operations (2 tests)

- [ ] Admin get user by ID → 200 OK
- [ ] Admin get all users with pagination → 200 OK + Paginated list
- [ ] Non-admin access admin endpoints → 403 Forbidden

---

## 📦 Product Service (28 tests)

### Product CRUD (10 tests)

- [ ] Create product với data hợp lệ → 201 Created
- [ ] Create product với name trống → 400 Bad Request
- [ ] Create product với price âm → 400 Bad Request
- [ ] Create product với slug trùng → 409 Conflict
- [ ] Get product by ID → 200 OK + Product details
- [ ] Get product by slug → 200 OK + Product details
- [ ] Get product không tồn tại → 404 Not Found
- [ ] Update product → 200 OK
- [ ] Update stock quantity → 200 OK
- [ ] Soft delete product → 204 No Content

### Product Search & Filter (10 tests)

- [ ] Get all products → 200 OK + List products
- [ ] Search products by name → 200 OK + Matching results
- [ ] Search với empty query → 200 OK + All products
- [ ] Filter by category → 200 OK + Products in category
- [ ] Filter by price range (min-max) → 200 OK + Products in range
- [ ] Filter by price với min > max → 200 OK + Empty list
- [ ] Filter by tags → 200 OK + Products with tags
- [ ] Filter by availability (in stock) → 200 OK + Available products
- [ ] Sort by price ascending → 200 OK + Correct order
- [ ] Sort by price descending → 200 OK + Correct order

### Product Features (5 tests)

- [ ] Get featured products → 200 OK + Only featured products
- [ ] Get related products → 200 OK + Same category products
- [ ] Get products by category ID → 200 OK + Category products
- [ ] Pagination (page 1, size 10) → 200 OK + 10 items
- [ ] Pagination với page > total pages → 200 OK + Empty list

### Category Management (3 tests)

- [ ] Create category → 201 Created
- [ ] Get all categories → 200 OK + List categories
- [ ] Update category → 200 OK
- [ ] Delete category → 204 No Content

---

## 🛒 Shopping Cart Service (22 tests)

### Cart Operations (10 tests)

- [ ] Get cart (first time) → 200 OK + Empty cart
- [ ] Add item to cart → 200 OK + Item added
- [ ] Add item với quantity = 0 → 400 Bad Request
- [ ] Add item với quantity âm → 400 Bad Request
- [ ] Add item với product không tồn tại → 404 Not Found
- [ ] Add same item twice → 200 OK + Quantity increased
- [ ] Update item quantity → 200 OK
- [ ] Update item với quantity > stock → 400 Bad Request
- [ ] Remove item from cart → 200 OK
- [ ] Clear cart → 200 OK + Empty cart

### Cart Calculations (5 tests)

- [ ] Calculate subtotal → Correct sum
- [ ] Calculate total với discount → Correct discounted total
- [ ] Apply discount code → 200 OK + Discount applied
- [ ] Apply invalid discount code → 400 Bad Request
- [ ] Apply expired discount code → 400 Bad Request

### Cart Validation (4 tests)

- [ ] Validate cart before checkout → 200 OK + All items valid
- [ ] Validate cart với out-of-stock item → 400 Bad Request
- [ ] Validate cart với deleted product → 400 Bad Request
- [ ] Validate cart với price changed → Warning + Updated prices

### Cart Persistence (3 tests)

- [ ] Cart persists after logout/login → Same cart data
- [ ] Cart expires after 30 days → Empty cart
- [ ] Merge guest cart with user cart after login → Combined cart

---

## 📋 Order Service (25 tests)

### Order Creation (8 tests)

- [ ] Create order from cart → 201 Created + Order ID
- [ ] Create order với empty cart → 400 Bad Request
- [ ] Create order với out-of-stock item → 400 Bad Request
- [ ] Create order without shipping address → 400 Bad Request
- [ ] Create order với invalid payment method → 400 Bad Request
- [ ] Calculate order total (items + shipping + tax) → Correct amount
- [ ] Create order → Cart cleared after success
- [ ] Create order → Stock reserved

### Order Management (8 tests)

- [ ] Get order by ID → 200 OK + Order details
- [ ] Get user orders → 200 OK + List orders
- [ ] Get user orders với pagination → 200 OK + Paginated list
- [ ] Filter orders by status → 200 OK + Filtered orders
- [ ] Filter orders by date range → 200 OK + Orders in range
- [ ] Update order status (Pending → Paid) → 200 OK
- [ ] Update order status (Paid → Shipped) → 200 OK
- [ ] Update order status (Shipped → Delivered) → 200 OK

### Order Cancellation (4 tests)

- [ ] Cancel pending order → 200 OK + Stock restored
- [ ] Cancel paid order → 400 Bad Request (Cannot cancel)
- [ ] Cancel shipped order → 400 Bad Request (Cannot cancel)
- [ ] Cancel already cancelled order → 400 Bad Request

### Order Tracking (3 tests)

- [ ] Get order tracking info → 200 OK + Tracking details
- [ ] Update tracking number → 200 OK
- [ ] Get order history/timeline → 200 OK + Status changes

### Admin Operations (2 tests)

- [ ] Admin get all orders → 200 OK + All orders
- [ ] Admin update any order status → 200 OK

---

## 💳 Payment Service (18 tests)

### Payment Processing (8 tests)

- [ ] Process payment với valid card → 200 OK + Payment success
- [ ] Process payment với invalid card → 400 Bad Request
- [ ] Process payment với expired card → 400 Bad Request
- [ ] Process payment với insufficient funds → 402 Payment Required
- [ ] Process payment với amount = 0 → 400 Bad Request
- [ ] Process payment với amount âm → 400 Bad Request
- [ ] Get payment status → 200 OK + Status
- [ ] Get payment by order ID → 200 OK + Payment details

### Payment Methods (4 tests)

- [ ] Pay with Credit Card → Success
- [ ] Pay with PayPal → Success
- [ ] Pay with Bank Transfer → Success
- [ ] Pay with invalid method → 400 Bad Request

### Refund (3 tests)

- [ ] Refund payment → 200 OK + Refund processed
- [ ] Refund already refunded payment → 400 Bad Request
- [ ] Partial refund → 200 OK + Partial amount refunded

### Webhooks (3 tests)

- [ ] Receive payment success webhook → Order updated
- [ ] Receive payment failed webhook → Order cancelled
- [ ] Receive webhook với invalid signature → 401 Unauthorized

---

## 📧 Notification Service (15 tests)

### Email Notifications (8 tests)

- [ ] Send welcome email after registration → Email sent
- [ ] Send email verification → Email sent with token
- [ ] Send password reset email → Email sent with token
- [ ] Send order confirmation email → Email sent with order details
- [ ] Send order shipped email → Email sent with tracking
- [ ] Send order delivered email → Email sent
- [ ] Send payment receipt → Email sent with invoice
- [ ] Send low stock alert to admin → Email sent

### SMS Notifications (3 tests)

- [ ] Send OTP via SMS → SMS sent
- [ ] Send order status update via SMS → SMS sent
- [ ] Send delivery notification via SMS → SMS sent

### Push Notifications (2 tests)

- [ ] Send push notification → Notification delivered
- [ ] Send push to offline user → Queued for delivery

### Notification Preferences (2 tests)

- [ ] User opt-out from email → No emails sent
- [ ] User opt-out from SMS → No SMS sent

---

## 🌐 API Gateway (10 tests)

### Routing (4 tests)

- [ ] Route to User Service → 200 OK
- [ ] Route to Product Service → 200 OK
- [ ] Route to Cart Service → 200 OK
- [ ] Route to non-existent service → 404 Not Found

### Authentication (3 tests)

- [ ] Request với valid JWT → Passed to service
- [ ] Request với invalid JWT → 401 Unauthorized
- [ ] Request với expired JWT → 401 Unauthorized

### Rate Limiting (2 tests)

- [ ] 100 requests in 1 minute → All pass
- [ ] 101st request in 1 minute → 429 Too Many Requests

### CORS (1 test)

- [ ] Request from allowed origin → Success
- [ ] Request from blocked origin → CORS error

---

## 🔄 Integration Tests (20 tests)

### User Journey (5 tests)

- [ ] Complete registration flow: Register → Verify → Login
- [ ] Complete password reset: Forgot → Reset → Login
- [ ] Complete profile update: Login → Update → Verify changes
- [ ] Complete address management: Add → Update → Set default → Delete
- [ ] Complete session management: Login → View sessions → Logout

### Shopping Journey (8 tests)

- [ ] Browse products → Search → Filter → View details
- [ ] Add to cart → Update quantity → Remove item → Clear cart
- [ ] Full checkout: Cart → Validate → Create order → Pay
- [ ] Order tracking: Create → Pay → Ship → Deliver
- [ ] Order cancellation: Create → Cancel → Verify stock restored
- [ ] Apply discount: Add items → Apply code → Verify discount
- [ ] Out of stock: Try add unavailable item → Error
- [ ] Price change: Item in cart → Price updated → Warning shown

### Cross-Service (7 tests)

- [ ] Create order → Product stock reduced
- [ ] Cancel order → Product stock restored
- [ ] Payment success → Order status updated → Email sent
- [ ] Order shipped → Tracking email sent → SMS sent
- [ ] User deleted → Orders preserved → Cart cleared
- [ ] Product deleted → Removed from carts → Orders unchanged
- [ ] Low stock → Admin notification sent

---

## 🚀 Performance Tests (10 tests)

### Load Tests

- [ ] 100 concurrent logins → All succeed
- [ ] 1000 product searches per minute → Response time < 500ms
- [ ] 500 concurrent cart operations → No data loss
- [ ] 100 concurrent order creations → All processed correctly

### Stress Tests

- [ ] 10,000 users registration in 1 hour → System stable
- [ ] 5,000 products created in 10 minutes → Database stable
- [ ] 1,000 orders per minute → Queue handles load

### Endurance Tests

- [ ] Run for 24 hours with normal load → No memory leaks
- [ ] Run for 1 week → Database size manageable
- [ ] Run for 1 month → Logs rotated properly

---

## 📝 Notes

### Testing Environment
- **Local:** http://localhost:5000
- **Staging:** TBD
- **Production:** TBD

### Test Data
- Test users: test1@example.com, test2@example.com
- Test products: Created via seed data
- Test cards: Use Stripe test cards

### Known Issues
- [ ] Issue #1: Description
- [ ] Issue #2: Description

### Next Steps
1. Setup test environment
2. Create .http files for each service
3. Run smoke tests
4. Document test results

---

**Legend:**
- [ ] Not tested
- [x] Passed
- [!] Failed
- [~] Skipped
- [?] Blocked

**Priority:**
- 🔴 Critical (Must test before deploy)
- 🟡 Important (Should test)
- 🟢 Nice to have (Can skip for MVP)
