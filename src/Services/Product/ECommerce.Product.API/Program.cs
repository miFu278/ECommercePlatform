using ECommerce.Product.Application.Validators;
using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using ECommerce.Product.Infrastructure.Repositories;
using ECommerce.Product.Infrastructure.Services;
using ECommerce.Shared.Abstractions.Interceptors;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ECommerce.Product.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel for multiple ports
            builder.WebHost.ConfigureKestrel(options =>
            {
                // REST API (HTTP/1.1)
                options.ListenLocalhost(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
                // gRPC (HTTP/2)
                options.ListenLocalhost(5011, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
            });

            // Add services to the container.
            builder.Services.AddControllers();
            
            // gRPC
            builder.Services.AddGrpc();
            
            // FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
            
            // AutoMapper
            builder.Services.AddAutoMapper(typeof(ECommerce.Product.Application.Mappings.ProductMappingProfile));

            // Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "ECommerce Product Service API",
                    Version = "v1",
                    Description = "Product Catalog API with MongoDB - Manage products, categories, and tags with advanced search and filtering",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "ECommerce Platform",
                        Email = "support@ecommerce.com"
                    }
                });

                // Enable XML comments (optional)
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // HttpContextAccessor for CurrentUserService
            builder.Services.AddHttpContextAccessor();

            // MongoDB Configuration
            var mongoSettings = new ECommerce.Product.Infrastructure.Configuration.MongoDbSettings
            {
                ConnectionString = builder.Configuration.GetConnectionString("MongoDB") 
                    ?? "mongodb://admin:admin123@localhost:27017",
                DatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ECommerce_Product"
            };

            builder.Services.AddSingleton(mongoSettings);
            builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();

            // Current User Service (for audit)
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            // Cloudinary Configuration
            var cloudinarySettings = new ECommerce.Product.Infrastructure.Configuration.CloudinarySettings
            {
                CloudName = builder.Configuration["Cloudinary:CloudName"] ?? "",
                ApiKey = builder.Configuration["Cloudinary:ApiKey"] ?? "",
                ApiSecret = builder.Configuration["Cloudinary:ApiSecret"] ?? ""
            };
            builder.Services.AddSingleton(cloudinarySettings);
            builder.Services.AddScoped<IImageService, ECommerce.Product.Infrastructure.Services.CloudinaryImageService>();

            // Application Services
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.IProductService, ECommerce.Product.Application.Services.ProductService>();
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.ICategoryService, ECommerce.Product.Application.Services.CategoryService>();
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.ITagService, ECommerce.Product.Application.Services.TagService>();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
                    options.RoutePrefix = string.Empty; // Swagger at root URL
                    options.DocumentTitle = "ECommerce Product Service API";
                    options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();
            
            // Map gRPC Service
            app.MapGrpcService<ECommerce.Product.API.Grpc.ProductGrpcService>();

            app.Run();
        }
    }
}
