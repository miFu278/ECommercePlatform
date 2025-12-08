var builder = DistributedApplication.CreateBuilder(args);

// External managed services - Supabase PostgreSQL
var userDb = builder.AddConnectionString("UserDb");
var orderDb = builder.AddConnectionString("OrderDb");
var paymentDb = builder.AddConnectionString("PaymentDb");

// MongoDB Atlas
var productDb = builder.AddConnectionString("ProductDb");
var notificationDb = builder.AddConnectionString("NotificationDb");

// Redis Cloud
var cartRedis = builder.AddConnectionString("CartRedis");

// CloudAMQP RabbitMQ
var rabbitMQ = builder.AddConnectionString("RabbitMQ");

// Services
var userApi = builder.AddProject<Projects.ECommerce_User_API>("user-api")
    .WithReference(userDb)
    .WithHttpEndpoint(port: 5010, name: "http")
    .WithHttpEndpoint(port: 5011, name: "grpc");

var productApi = builder.AddProject<Projects.ECommerce_Product_API>("product-api")
    .WithReference(productDb)
    .WithHttpEndpoint(port: 5020, name: "http")
    .WithHttpEndpoint(port: 5021, name: "grpc");

var cartApi = builder.AddProject<Projects.ECommerce_ShoppingCart_API>("cart-api")
    .WithReference(cartRedis)
    .WithReference(productApi)
    .WithHttpEndpoint(port: 5030, name: "http");

var orderApi = builder.AddProject<Projects.ECommerce_Order_API>("order-api")
    .WithReference(orderDb)
    .WithReference(rabbitMQ)
    .WithReference(cartApi)
    .WithReference(productApi)
    .WithReference(userApi)
    .WithHttpEndpoint(port: 5040, name: "http")
    .WithHttpEndpoint(port: 5041, name: "grpc");

var paymentApi = builder.AddProject<Projects.ECommerce_Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbitMQ)
    .WithReference(orderApi)
    .WithHttpEndpoint(port: 5050, name: "http");

var notificationApi = builder.AddProject<Projects.ECommerce_Notification_API>("notification-api")
    .WithReference(notificationDb)
    .WithReference(rabbitMQ)
    .WithHttpEndpoint(port: 5060, name: "http");

// API Gateway
builder.AddProject<Projects.ECommerce_ApiGateway>("apigateway")
    .WithReference(userApi)
    .WithReference(productApi)
    .WithReference(cartApi)
    .WithReference(orderApi)
    .WithReference(paymentApi)
    .WithReference(notificationApi)
    .WithHttpEndpoint(port: 5000, name: "http");

builder.Build().Run();
