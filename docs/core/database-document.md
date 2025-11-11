# E-Commerce Platform - Database Design Documentation

## Table of Contents
1. [Database Overview](#database-overview)
2. [User Service Database (PostgreSQL)](#user-service-database-postgresql)
3. [Product Catalog Database (MongoDB)](#product-catalog-database-mongodb)
4. [Shopping Cart Database (Redis)](#shopping-cart-database-redis)
5. [Order Service Database (PostgreSQL)](#order-service-database-postgresql)
6. [Payment Service Database (PostgreSQL)](#payment-service-database-postgresql)
7. [Notification Service Database (MongoDB)](#notification-service-database-mongodb)
8. [Database Relationships](#database-relationships)
9. [Indexing Strategy](#indexing-strategy)
10. [Data Migration](#data-migration)
11. [Backup and Restore](#backup-and-restore)

---

## Database Overview

### Database Selection Rationale

| Service | Database | Reason |
|---------|----------|--------|
| User Service | PostgreSQL | ACID compliance, relational data, strong consistency |
| Product Catalog | MongoDB | Flexible schema, read-heavy, complex queries |
| Shopping Cart | Redis | In-memory speed, TTL support, session data |
| Order Service | PostgreSQL | Transactional integrity, audit trail |
| Payment Service | PostgreSQL | Financial data, ACID compliance, strict consistency |
| Notification Service | MongoDB | Flexible schema, high write volume, log storage |

### Connection Strings

```json
{
  "ConnectionStrings": {
    "UserDb": "Host=postgres;Port=5432;Database=UserDb;Username=postgres;Password=password",
    "OrderDb": "Host=postgres;Port=5432;Database=OrderDb;Username=postgres;Password=password",
    "PaymentDb": "Host=postgres;Port=5432;Database=PaymentDb;Username=postgres;Password=password",
    "ProductDb": "mongodb://mongodb:27017/ProductDb",
    "NotificationDb": "mongodb://mongodb:27017/NotificationDb",
    "Redis": "redis:6379"
  }
}
```

---

## User Service Database (PostgreSQL)

### Database: UserDb

#### Tables Overview
- users
- roles
- user_roles
- addresses
- user_sessions
- audit_logs

#### Users Table
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    username VARCHAR(100) UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone_number VARCHAR(20),
    date_of_birth DATE,
    email_verified BOOLEAN DEFAULT FALSE,
    email_verification_token VARCHAR(500),
    email_verification_token_expires TIMESTAMP,
    password_reset_token VARCHAR(500),
    password_reset_token_expires TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_locked BOOLEAN DEFAULT FALSE,
    failed_login_attempts INT DEFAULT 0,
    lockout_end TIMESTAMP,
    last_login_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL,
    
    CONSTRAINT email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

CREATE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_username ON users(username) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_email_verification_token ON users(email_verification_token);
CREATE INDEX idx_users_password_reset_token ON users(password_reset_token);
CREATE INDEX idx_users_created_at ON users(created_at);
```

#### Roles Table
```sql
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT role_name_check CHECK (name IN ('Admin', 'Manager', 'Customer', 'Guest'))
);

INSERT INTO roles (name, description) VALUES
    ('Admin', 'Full system access'),
    ('Manager', 'Product and order management'),
    ('Customer', 'Standard customer access'),
    ('Guest', 'Limited browsing access');
```

#### User_Roles Table (Many-to-Many)
```sql
CREATE TABLE user_roles (
    user_id UUID NOT NULL,
    role_id INT NOT NULL,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    assigned_by UUID,
    
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    FOREIGN KEY (assigned_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX idx_user_roles_role_id ON user_roles(role_id);
```

#### Addresses Table
```sql
CREATE TABLE addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    address_type VARCHAR(20) DEFAULT 'shipping',
    street_address VARCHAR(255) NOT NULL,
    apartment VARCHAR(50),
    city VARCHAR(100) NOT NULL,
    state_province VARCHAR(100),
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT address_type_check CHECK (address_type IN ('shipping', 'billing'))
);

CREATE INDEX idx_addresses_user_id ON addresses(user_id);
CREATE INDEX idx_addresses_is_default ON addresses(is_default) WHERE is_default = TRUE;
```

#### User_Sessions Table
```sql
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    refresh_token VARCHAR(500) NOT NULL UNIQUE,
    device_info JSONB,
    ip_address INET,
    user_agent TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_accessed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_refresh_token ON user_sessions(refresh_token);
CREATE INDEX idx_user_sessions_expires_at ON user_sessions(expires_at);
```

#### Audit_Logs Table
```sql
CREATE TABLE audit_logs (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID,
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100),
    entity_id VARCHAR(255),
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
```

---

## Product Catalog Database (MongoDB)

### Database: ProductDb

#### Collections Overview
- products
- categories
- product_reviews
- inventory_logs

#### Products Collection
```javascript
{
  _id: ObjectId,
  sku: String,           // Unique stock keeping unit
  name: String,
  slug: String,          // URL-friendly name
  description: String,
  long_description: String,
  
  // Pricing
  price: Decimal128,
  compare_at_price: Decimal128,
  cost_price: Decimal128,
  
  // Inventory
  stock: Number,
  low_stock_threshold: Number,
  track_inventory: Boolean,
  
  // Category
  category_id: ObjectId,
  category_path: [ObjectId],  // For hierarchical categories
  
  // Images
  images: [{
    url: String,
    alt_text: String,
    is_primary: Boolean,
    order: Number
  }],
  
  // Attributes
  attributes: [{
    name: String,
    value: String
  }],
  
  // Specifications
  specifications: {
    brand: String,
    model: String,
    weight: String,
    dimensions: {
      length: Number,
      width: Number,
      height: Number,
      unit: String
    },
    warranty: String
  },
  
  // SEO
  seo: {
    meta_title: String,
    meta_description: String,
    meta_keywords: [String]
  },
  
  // Status
  status: String,         // draft, active, archived
  is_featured: Boolean,
  is_published: Boolean,
  published_at: Date,
  
  // Tags
  tags: [String],
  
  // Reviews
  rating: {
    average: Number,
    count: Number
  },
  
  // Timestamps
  created_at: Date,
  updated_at: Date,
  deleted_at: Date
}

// Indexes
db.products.createIndex({ "sku": 1 }, { unique: true })
db.products.createIndex({ "slug": 1 }, { unique: true })
db.products.createIndex({ "name": "text", "description": "text", "tags": "text" })
db.products.createIndex({ "category_id": 1 })
db.products.createIndex({ "status": 1, "is_published": 1 })
db.products.createIndex({ "price": 1 })
db.products.createIndex({ "created_at": -1 })
db.products.createIndex({ "rating.average": -1 })
```

#### Categories Collection
```javascript
{
  _id: ObjectId,
  name: String,
  slug: String,
  description: String,
  parent_id: ObjectId,       // null for root categories
  level: Number,             // 0 for root, 1 for child, etc.
  path: [ObjectId],          // Array of ancestor IDs
  image: String,
  icon: String,
  order: Number,
  is_active: Boolean,
  seo: {
    meta_title: String,
    meta_description: String
  },
  created_at: Date,
  updated_at: Date
}

// Indexes
db.categories.createIndex({ "slug": 1 }, { unique: true })
db.categories.createIndex({ "parent_id": 1 })
db.categories.createIndex({ "path": 1 })
db.categories.createIndex({ "order": 1 })
```

#### Product_Reviews Collection
```javascript
{
  _id: ObjectId,
  product_id: ObjectId,
  user_id: String,           // UUID from User Service
  order_id: String,          // UUID from Order Service
  rating: Number,            // 1-5
  title: String,
  comment: String,
  images: [String],
  verified_purchase: Boolean,
  helpful_count: Number,
  reported_count: Number,
  status: String,            // pending, approved, rejected
  response: {
    text: String,
    responded_by: String,
    responded_at: Date
  },
  created_at: Date,
  updated_at: Date
}

// Indexes
db.product_reviews.createIndex({ "product_id": 1, "user_id": 1 }, { unique: true })
db.product_reviews.createIndex({ "product_id": 1, "status": 1 })
db.product_reviews.createIndex({ "rating": 1 })
db.product_reviews.createIndex({ "created_at": -1 })
```

#### Inventory_Logs Collection
```javascript
{
  _id: ObjectId,
  product_id: ObjectId,
  sku: String,
  type: String,              // purchase, sale, adjustment, return
  quantity: Number,          // Positive or negative
  previous_stock: Number,
  new_stock: Number,
  reference_id: String,      // Order ID, Purchase Order ID, etc.
  reference_type: String,
  notes: String,
  performed_by: String,      // User ID
  created_at: Date
}

// Indexes
db.inventory_logs.createIndex({ "product_id": 1, "created_at": -1 })
db.inventory_logs.createIndex({ "sku": 1, "created_at": -1 })
db.inventory_logs.createIndex({ "type": 1 })
db.inventory_logs.createIndex({ "reference_id": 1 })
```

---

## Shopping Cart Database (Redis)

### Key-Value Structure

#### Cart Key Pattern
```
cart:{userId}
```

#### Cart Data Structure (Hash)
```
Fields:
- items: JSON array of cart items
- subtotal: decimal
- discount: decimal
- total: decimal
- coupon_code: string
- created_at: timestamp
- updated_at: timestamp
```

#### Cart Item JSON Structure
```json
{
  "product_id": "prod-001",
  "sku": "WH-001",
  "name": "Wireless Headphones",
  "image": "https://cdn.example.com/products/wh-001.jpg",
  "price": 299.99,
  "quantity": 2,
  "subtotal": 599.98,
  "added_at": "2025-11-02T10:30:00Z"
}
```

#### Redis Commands Examples
```bash
# Set cart with expiration (24 hours = 86400 seconds)
HSET cart:user-123 items '[{"product_id":"prod-001","quantity":2,"price":299.99}]'
HSET cart:user-123 subtotal 599.98
HSET cart:user-123 total 599.98
HSET cart:user-123 created_at "2025-11-02T10:30:00Z"
EXPIRE cart:user-123 86400

# Get entire cart
HGETALL cart:user-123

# Get specific field
HGET cart:user-123 items

# Update cart
HSET cart:user-123 items '[{"product_id":"prod-001","quantity":3,"price":299.99}]'
HSET cart:user-123 subtotal 899.97
HSET cart:user-123 total 899.97

# Apply coupon
HSET cart:user-123 coupon_code "SAVE20"
HSET cart:user-123 discount 179.99
HSET cart:user-123 total 719.98

# Delete cart
DEL cart:user-123

# Check if cart exists
EXISTS cart:user-123

# Get cart TTL (time to live)
TTL cart:user-123

# Extend cart expiration
EXPIRE cart:user-123 86400
```

#### Session Keys
```
session:{sessionId}  - User session data
wishlist:{userId}    - User wishlist
recently_viewed:{userId}  - Recently viewed products
```

---

## Order Service Database (PostgreSQL)

### Database: OrderDb

#### Tables Overview
- orders
- order_items
- order_status_history
- shipping_info
- coupons

#### Orders Table
```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number VARCHAR(50) NOT NULL UNIQUE,
    user_id UUID NOT NULL,
    
    -- Order status
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    
    -- Pricing
    subtotal DECIMAL(10, 2) NOT NULL,
    discount DECIMAL(10, 2) DEFAULT 0,
    tax DECIMAL(10, 2) DEFAULT 0,
    shipping_cost DECIMAL(10, 2) DEFAULT 0,
    total DECIMAL(10, 2) NOT NULL,
    
    -- Coupon
    coupon_code VARCHAR(50),
    coupon_discount DECIMAL(10, 2) DEFAULT 0,
    
    -- Addresses (denormalized for historical record)
    shipping_address JSONB NOT NULL,
    billing_address JSONB NOT NULL,
    
    -- Shipping
    shipping_method VARCHAR(100),
    tracking_number VARCHAR(100),
    carrier VARCHAR(100),
    estimated_delivery_date DATE,
    actual_delivery_date DATE,
    
    -- Payment
    payment_method VARCHAR(50),
    payment_status VARCHAR(50) DEFAULT 'pending',
    payment_id UUID,
    
    -- Notes
    customer_notes TEXT,
    admin_notes TEXT,
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    cancelled_at TIMESTAMP,
    completed_at TIMESTAMP,
    
    CONSTRAINT order_status_check CHECK (status IN ('pending', 'paid', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded')),
    CONSTRAINT payment_status_check CHECK (payment_status IN ('pending', 'paid', 'failed', 'refunded'))
);

CREATE INDEX idx_orders_user_id ON orders(user_id);
CREATE INDEX idx_orders_order_number ON orders(order_number);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_created_at ON orders(created_at DESC);
CREATE INDEX idx_orders_payment_status ON orders(payment_status);
```

#### Order_Items Table
```sql
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    product_id VARCHAR(100) NOT NULL,  -- From Product Service
    sku VARCHAR(100) NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    product_image VARCHAR(500),
    
    -- Pricing (snapshot at time of order)
    price DECIMAL(10, 2) NOT NULL,
    quantity INT NOT NULL,
    subtotal DECIMAL(10, 2) NOT NULL,
    discount DECIMAL(10, 2) DEFAULT 0,
    total DECIMAL(10, 2) NOT NULL,
    
    -- Product attributes at time of order
    attributes JSONB,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_order_items_product_id ON order_items(product_id);
CREATE INDEX idx_order_items_sku ON order_items(sku);
```

#### Order_Status_History Table
```sql
CREATE TABLE order_status_history (
    id BIGSERIAL PRIMARY KEY,
    order_id UUID NOT NULL,
    old_status VARCHAR(50),
    new_status VARCHAR(50) NOT NULL,
    notes TEXT,
    changed_by UUID,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE INDEX idx_order_status_history_order_id ON order_status_history(order_id);
CREATE INDEX idx_order_status_history_changed_at ON order_status_history(changed_at);
```

#### Shipping_Info Table
```sql
CREATE TABLE shipping_info (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL UNIQUE,
    carrier VARCHAR(100),
    tracking_number VARCHAR(100),
    shipping_method VARCHAR(100),
    shipped_at TIMESTAMP,
    estimated_delivery TIMESTAMP,
    actual_delivery TIMESTAMP,
    tracking_url VARCHAR(500),
    tracking_updates JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
);

CREATE INDEX idx_shipping_info_order_id ON shipping_info(order_id);
CREATE INDEX idx_shipping_info_tracking_number ON shipping_info(tracking_number);
```

#### Coupons Table
```sql
CREATE TABLE coupons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    discount_type VARCHAR(20) NOT NULL,  -- percentage, fixed
    discount_value DECIMAL(10, 2) NOT NULL,
    min_order_amount DECIMAL(10, 2),
    max_discount_amount DECIMAL(10, 2),
    usage_limit INT,
    usage_count INT DEFAULT 0,
    per_user_limit INT DEFAULT 1,
    is_active BOOLEAN DEFAULT TRUE,
    valid_from TIMESTAMP NOT NULL,
    valid_until TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT discount_type_check CHECK (discount_type IN ('percentage', 'fixed'))
);

CREATE INDEX idx_coupons_code ON coupons(code);
CREATE INDEX idx_coupons_is_active ON coupons(is_active);
CREATE INDEX idx_coupons_valid_dates ON coupons(valid_from, valid_until);
```

---

## Payment Service Database (PostgreSQL)

### Database: PaymentDb

#### Tables Overview
- payments
- payment_methods
- refunds
- transactions

#### Payments Table
```sql
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    user_id UUID NOT NULL,
    
    -- Payment details
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    payment_method VARCHAR(50) NOT NULL,
    
    -- Gateway information
    gateway VARCHAR(50) NOT NULL,  -- stripe, paypal
    gateway_transaction_id VARCHAR(255),
    gateway_response JSONB,
    
    -- Payment method details (encrypted/tokenized)
    payment_method_id UUID,
    card_last_four VARCHAR(4),
    card_brand VARCHAR(50),
    
    -- Metadata
    ip_address INET,
    user_agent TEXT,
    metadata JSONB,
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    paid_at TIMESTAMP,
    failed_at TIMESTAMP,
    
    CONSTRAINT payment_status_check CHECK (status IN ('pending', 'processing', 'succeeded', 'failed', 'cancelled', 'refunded')),
    CONSTRAINT payment_method_check CHECK (payment_method IN ('credit_card', 'debit_card', 'paypal', 'bank_transfer'))
);

CREATE INDEX idx_payments_order_id ON payments(order_id);
CREATE INDEX idx_payments_user_id ON payments(user_id);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_payments_gateway_transaction_id ON payments(gateway_transaction_id);
CREATE INDEX idx_payments_created_at ON payments(created_at DESC);
```

#### Payment_Methods Table
```sql
CREATE TABLE payment_methods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL,
    
    -- Card details (tokenized)
    card_token VARCHAR(500),
    card_last_four VARCHAR(4),
    card_brand VARCHAR(50),
    card_exp_month INT,
    card_exp_year INT,
    cardholder_name VARCHAR(255),
    
    -- PayPal
    paypal_email VARCHAR(255),
    paypal_account_id VARCHAR(255),
    
    -- Bank transfer
    bank_name VARCHAR(255),
    account_number_last_four VARCHAR(4),
    
    -- Status
    is_default BOOLEAN DEFAULT FALSE,
    is_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT payment_method_type_check CHECK (type IN ('credit_card', 'debit_card', 'paypal', 'bank_transfer'))
);

CREATE INDEX idx_payment_methods_user_id ON payment_methods(user_id);
CREATE INDEX idx_payment_methods_is_default ON payment_methods(is_default) WHERE is_default = TRUE;
```

#### Refunds Table
```sql
CREATE TABLE refunds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL,
    order_id UUID NOT NULL,
    
    -- Refund details
    amount DECIMAL(10, 2) NOT NULL,
    reason VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    
    -- Gateway information
    gateway_refund_id VARCHAR(255),
    gateway_response JSONB,
    
    -- Metadata
    requested_by UUID,
    approved_by UUID,
    notes TEXT,
    
    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP,
    completed_at TIMESTAMP,
    
    FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE CASCADE,
    CONSTRAINT refund_status_check CHECK (status IN ('pending', 'processing', 'completed', 'failed', 'cancelled'))
);

CREATE INDEX idx_refunds_payment_id ON refunds(payment_id);
CREATE INDEX idx_refunds_order_id ON refunds(order_id);
CREATE INDEX idx_refunds_status ON refunds(status);
CREATE INDEX idx_refunds_created_at ON refunds(created_at DESC);
```

#### Transactions Table (Audit Log)
```sql
CREATE TABLE transactions (
    id BIGSERIAL PRIMARY KEY,
    payment_id UUID,
    type VARCHAR(50) NOT NULL,  -- charge, refund, authorization, capture
    amount DECIMAL(10, 2) NOT NULL,
    status VARCHAR(50) NOT NULL,
    gateway VARCHAR(50) NOT NULL,
    gateway_transaction_id VARCHAR(255),
    request_data JSONB,
    response_data JSONB,
    error_message TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE SET NULL
);

CREATE INDEX idx_transactions_payment_id ON transactions(payment_id);
CREATE INDEX idx_transactions_type ON transactions(type);
CREATE INDEX idx_transactions_status ON transactions(status);
CREATE INDEX idx_transactions_created_at ON transactions(created_at DESC);
```

---

## Notification Service Database (MongoDB)

### Database: NotificationDb

#### Collections Overview
- notifications
- notification_templates
- email_logs
- sms_logs

#### Notifications Collection
```javascript
{
  _id: ObjectId,
  user_id: String,           // UUID from User Service
  type: String,              // email, sms, push
  channel: String,           // order, payment, marketing, system
  subject: String,
  message: String,
  template_id: ObjectId,
  template_data: Object,
  
  // Recipient info
  recipient: {
    email: String,
    phone: String,
    device_token: String
  },
  
  // Status
  status: String,            // queued, sent, delivered, failed, bounced
  attempts: Number,
  max_attempts: Number,
  
  // Delivery info
  sent_at: Date,
  delivered_at: Date,
  opened_at: Date,
  clicked_at: Date,
  failed_at: Date,
  
  // Error tracking
  error: {
    code: String,
    message: String,
    details: Object
  },
  
  // Metadata
  priority: String,          // low, normal, high, urgent
  metadata: Object,
  
  created_at: Date,
  updated_at: Date
}

// Indexes
db.notifications.createIndex({ "user_id": 1, "created_at": -1 })
db.notifications.createIndex({ "status": 1 })
db.notifications.createIndex({ "type": 1, "status": 1 })
db.notifications.createIndex({ "created_at": -1 })
db.notifications.createIndex({ "sent_at": -1 })
```

#### Notification_Templates Collection
```javascript
{
  _id: ObjectId,
  name: String,
  code: String,              // Unique identifier
  type: String,              // email, sms, push
  subject: String,
  body: String,              // Template with placeholders
  html_body: String,         // For email
  
  // Variables
  variables: [String],       // List of required variables
  
  // Settings
  is_active: Boolean,
  language: String,
  
  created_at: Date,
  updated_at: Date
}

// Indexes
db.notification_templates.createIndex({ "code": 1 }, { unique: true })
db.notification_templates.createIndex({ "type": 1, "is_active": 1 })
```

#### Email_Logs Collection
```javascript
{
  _id: ObjectId,
  notification_id: ObjectId,
  user_id: String,
  
  // Email details
  from: String,
  to: String,
  cc: [String],
  bcc: [String],
  subject: String,
  body_text: String,
  body_html: String,
  
  // Attachments
  attachments: [{
    filename: String,
    url: String,
    size: Number
  }],
  
  // Provider info
  provider: String,          // sendgrid, aws-ses, smtp
  provider_message_id: String,
  
  // Status
  status: String,
  sent_at: Date,
  delivered_at: Date,
  opened_at: Date,
  clicked_at: Date,
  bounced_at: Date,
  
  // Tracking
  open_count: Number,
  click_count: Number,
  
  // Error
  error: {
    code: String,
    message: String
  },
  
  created_at: Date
}

// Indexes
db.email_logs.createIndex({ "notification_id": 1 })
db.email_logs.createIndex({ "user_id": 1, "created_at": -1 })
db.email_logs.createIndex({ "to": 1 })
db.email_logs.createIndex({ "status": 1 })
db.email_logs.createIndex({ "created_at": -1 })
```

#### SMS_Logs Collection
```javascript
{
  _id: ObjectId,
  notification_id: ObjectId,
  user_id: String,
  
  // SMS details
  from: String,
  to: String,
  message: String,
  
  // Provider info
  provider: String,          // twilio, aws-sns
  provider_message_id: String,
  
  // Status
  status: String,
  sent_at: Date,
  delivered_at: Date,
  failed_at: Date,
  
  // Cost
  segments: Number,
  cost: Number,
  currency: String,
  
  // Error
  error: {
    code: String,
    message: String
  },
  
  created_at: Date
}

// Indexes
db.sms_logs.createIndex({ "notification_id": 1 })
db.sms_logs.createIndex({ "user_id": 1, "created_at": -1 })
db.sms_logs.createIndex({ "to": 1 })
db.sms_logs.createIndex({ "status": 1 })
db.sms_logs.createIndex({ "created_at": -1 })
```

---

## Database Relationships

### Cross-Service Data References

Since each microservice has its own database, relationships across services are maintained through:

1. **User ID References**: All services store `user_id` as UUID string
2. **Product ID References**: Order and Cart services reference products by ID
3. **Order ID References**: Payment and Notification services reference orders
4. **Event-Driven Sync**: Services communicate via events to maintain consistency

### Example Data Flow

```
User Service (user_id: uuid-123)
    ↓
Order Service (order with user_id: uuid-123)
    ↓
Payment Service (payment with order_id and user_id)
    ↓
Notification Service (notification with user_id and order_id)
```

### Data Consistency Strategies

1. **Eventual Consistency**: Accept temporary inconsistencies
2. **Saga Pattern**: Coordinate distributed transactions
3. **Event Sourcing**: Store all changes as events
4. **CQRS**: Separate read and write models

---

## Indexing Strategy

### PostgreSQL Indexes

**Performance Guidelines:**
- Index foreign keys
- Index columns used in WHERE clauses
- Index columns used in ORDER BY
- Index columns used in JOIN conditions
- Use partial indexes for filtered queries
- Avoid over-indexing (impacts write performance)

**Monitoring:**
```sql
-- Find missing indexes
SELECT schemaname, tablename, attname, n_distinct, correlation
FROM pg_stats
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY abs(correlation) DESC;

-- Find unused indexes
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE idx_scan = 0
ORDER BY schemaname, tablename;
```

### MongoDB Indexes

**Performance Guidelines:**
- Create compound indexes for common query patterns
- Use text indexes for full-text search
- Index fields used in sort operations
- Monitor index usage with `db.collection.stats()`
- Remove unused indexes

**Monitoring:**
```javascript
// Check index usage
db.products.aggregate([
  { $indexStats: {} }
])

// Explain query plan
db.products.find({ status: "active" }).explain("executionStats")
```

### Redis Performance

**Best Practices:**
- Use appropriate data structures (Hash for carts)
- Set TTL on temporary data
- Use pipelining for multiple operations
- Monitor memory usage
- Use Redis Cluster for scaling

---

## Data Migration

### PostgreSQL Migrations (Entity Framework Core)

**Create Migration:**
```bash
# User Service
cd src/Services/Users/ECommerce.User.API
dotnet ef migrations add InitialCreate --project ../ECommerce.User.Infrastructure

# Apply migration
dotnet ef database update
```

**Migration Script Example:**
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                email = table.Column<string>(maxLength: 255, nullable: false),
                password_hash = table.Column<string>(maxLength: 500, nullable: false),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
            });
            
        migrationBuilder.CreateIndex(
            name: "idx_users_email",
            table: "users",
            column: "email",
            unique: true);
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "users");
    }
}
```

### MongoDB Migrations

**Schema Validation:**
```javascript
db.createCollection("products", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["sku", "name", "price", "stock"],
      properties: {
        sku: {
          bsonType: "string",
          description: "must be a string and is required"
        },
        name: {
          bsonType: "string",
          description: "must be a string and is required"
        },
        price: {
          bsonType: "decimal",
          minimum: 0,
          description: "must be a decimal and >= 0"
        }
      }
    }
  }
})
```

### Data Seeding

**PostgreSQL Seed Data:**
```sql
-- Seed roles
INSERT INTO roles (name, description) VALUES
    ('Admin', 'Full system access'),
    ('Manager', 'Product and order management'),
    ('Customer', 'Standard customer access'),
    ('Guest', 'Limited browsing access')
ON CONFLICT (name) DO NOTHING;

-- Seed admin user
INSERT INTO users (id, email, password_hash, first_name, last_name, email_verified, is_active)
VALUES (
    gen_random_uuid(),
    'admin@ecommerce.com',
    '$2a$11$hashed_password_here',
    'Admin',
    'User',
    TRUE,
    TRUE
) ON CONFLICT (email) DO NOTHING;
```

**MongoDB Seed Data:**
```javascript
// Seed categories
db.categories.insertMany([
  {
    name: "Electronics",
    slug: "electronics",
    description: "Electronic devices and accessories",
    parent_id: null,
    level: 0,
    is_active: true,
    created_at: new Date()
  },
  {
    name: "Computers",
    slug: "computers",
    parent_id: ObjectId("electronics_id"),
    level: 1,
    is_active: true,
    created_at: new Date()
  }
])
```

---

## Backup and Restore

### PostgreSQL Backup

**Full Database Backup:**
```bash
# Backup single database
pg_dump -h localhost -U postgres -d UserDb -F c -f userdb_backup.dump

# Backup all databases
pg_dumpall -h localhost -U postgres -f all_databases_backup.sql

# Automated daily backup script
#!/bin/bash
BACKUP_DIR="/backups/postgres"
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -h localhost -U postgres -d UserDb -F c -f "$BACKUP_DIR/userdb_$DATE.dump"

# Keep only last 7 days
find $BACKUP_DIR -name "userdb_*.dump" -mtime +7 -delete
```

**Restore:**
```bash
# Restore database
pg_restore -h localhost -U postgres -d UserDb -c userdb_backup.dump

# Restore from SQL file
psql -h localhost -U postgres -d UserDb -f backup.sql
```

### MongoDB Backup

**Full Database Backup:**
```bash
# Backup single database
mongodump --host localhost --port 27017 --db ProductDb --out /backups/mongodb

# Backup with authentication
mongodump --host localhost --port 27017 --username admin --password password --authenticationDatabase admin --db ProductDb --out /backups/mongodb

# Automated backup script
#!/bin/bash
BACKUP_DIR="/backups/mongodb"
DATE=$(date +%Y%m%d_%H%M%S)
mongodump --host localhost --db ProductDb --out "$BACKUP_DIR/$DATE"

# Compress backup
tar -czf "$BACKUP_DIR/productdb_$DATE.tar.gz" "$BACKUP_DIR/$DATE"
rm -rf "$BACKUP_DIR/$DATE"

# Keep only last 7 days
find $BACKUP_DIR -name "productdb_*.tar.gz" -mtime +7 -delete
```

**Restore:**
```bash
# Restore database
mongorestore --host localhost --port 27017 --db ProductDb /backups/mongodb/ProductDb

# Restore from compressed backup
tar -xzf productdb_backup.tar.gz
mongorestore --host localhost --db ProductDb ./ProductDb
```

### Redis Backup

**RDB Snapshot:**
```bash
# Manual snapshot
redis-cli SAVE

# Background snapshot
redis-cli BGSAVE

# Configure automatic snapshots in redis.conf
save 900 1      # Save after 900 seconds if at least 1 key changed
save 300 10     # Save after 300 seconds if at least 10 keys changed
save 60 10000   # Save after 60 seconds if at least 10000 keys changed
```

**AOF (Append Only File):**
```bash
# Enable AOF in redis.conf
appendonly yes
appendfilename "appendonly.aof"

# Rewrite AOF
redis-cli BGREWRITEAOF
```

**Restore:**
```bash
# Copy dump.rdb or appendonly.aof to Redis data directory
cp dump.rdb /var/lib/redis/
# Restart Redis
systemctl restart redis
```

### Backup Strategy

**Recommended Schedule:**
- **Full Backup**: Daily at 2 AM
- **Incremental Backup**: Every 6 hours
- **Retention**: Keep 7 daily, 4 weekly, 12 monthly backups
- **Off-site Backup**: Sync to cloud storage (S3, Azure Blob)

**Backup Verification:**
```bash
# Test restore on separate instance
# Verify data integrity
# Check backup size and completion
```

---

## Database Performance Optimization

### PostgreSQL Optimization

**Configuration (postgresql.conf):**
```ini
# Memory
shared_buffers = 4GB
effective_cache_size = 12GB
work_mem = 64MB
maintenance_work_mem = 1GB

# Connections
max_connections = 200

# WAL
wal_buffers = 16MB
checkpoint_completion_target = 0.9

# Query Planning
random_page_cost = 1.1
effective_io_concurrency = 200
```

**Vacuum and Analyze:**
```sql
-- Manual vacuum
VACUUM ANALYZE users;

-- Auto-vacuum configuration
ALTER TABLE users SET (autovacuum_vacuum_scale_factor = 0.1);
```

### MongoDB Optimization

**Configuration (mongod.conf):**
```yaml
storage:
  wiredTiger:
    engineConfig:
      cacheSizeGB: 4
    collectionConfig:
      blockCompressor: snappy

operationProfiling:
  mode: slowOp
  slowOpThresholdMs: 100
```

**Query Optimization:**
```javascript
// Use projection to limit fields
db.products.find(
  { status: "active" },
  { name: 1, price: 1, _id: 0 }
)

// Use covered queries (query uses only indexed fields)
db.products.find(
  { sku: "WH-001" },
  { sku: 1, _id: 0 }
).hint({ sku: 1 })
```

### Redis Optimization

**Configuration (redis.conf):**
```ini
# Memory
maxmemory 2gb
maxmemory-policy allkeys-lru

# Persistence
save 900 1
save 300 10
save 60 10000

# Performance
tcp-backlog 511
timeout 300
```

---

## Security Best Practices

### PostgreSQL Security

1. **Use SSL/TLS connections**
2. **Strong passwords and rotate regularly**
3. **Principle of least privilege**
4. **Encrypt sensitive data at rest**
5. **Regular security updates**
6. **Audit logging enabled**

```sql
-- Create read-only user
CREATE USER readonly_user WITH PASSWORD 'strong_password';
GRANT CONNECT ON DATABASE UserDb TO readonly_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;

-- Revoke unnecessary permissions
REVOKE ALL ON DATABASE UserDb FROM PUBLIC;
```

### MongoDB Security

1. **Enable authentication**
2. **Use role-based access control**
3. **Enable encryption at rest**
4. **Use TLS for connections**
5. **Regular backups**

```javascript
// Create application user
db.createUser({
  user: "app_user",
  pwd: "strong_password",
  roles: [
    { role: "readWrite", db: "ProductDb" }
  ]
})
```

### Redis Security

1. **Require password (requirepass)**
2. **Bind to specific IP**
3. **Disable dangerous commands**
4. **Use TLS for connections**
5. **Regular backups**

```ini
# redis.conf
requirepass your_strong_password
bind 127.0.0.1
rename-command FLUSHDB ""
rename-command FLUSHALL ""
```

---

## Monitoring and Maintenance

### Health Checks

**PostgreSQL:**
```sql
-- Check database size
SELECT pg_database.datname, pg_size_pretty(pg_database_size(pg_database.datname))
FROM pg_database;

-- Check table sizes
SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Check active connections
SELECT count(*) FROM pg_stat_activity;
```

**MongoDB:**
```javascript
// Database stats
db.stats()

// Collection stats
db.products.stats()

// Current operations
db.currentOp()
```

**Redis:**
```bash
# Server info
redis-cli INFO

# Memory usage
redis-cli INFO memory

# Check keys
redis-cli DBSIZE
```

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Maintained By**: Database Team
