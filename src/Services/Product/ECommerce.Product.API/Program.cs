using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using ECommerce.Product.Infrastructure.Repositories;
using ECommerce.Product.Infrastructure.Services;
using ECommerce.Shared.Abstractions.Interceptors;

namespace ECommerce.Product.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
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
                    Description = "Product Catalog API with MongoDB - Manage products, categories, and tags",
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

            // Application Services
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.IProductService, ECommerce.Product.Application.Services.ProductService>();
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.ICategoryService, ECommerce.Product.Application.Services.CategoryService>();
            builder.Services.AddScoped<ECommerce.Product.Application.Interfaces.ITagService, ECommerce.Product.Application.Services.TagService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
                    options.RoutePrefix = string.Empty; // Swagger at root URL
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
