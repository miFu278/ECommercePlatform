using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using ECommerce.Product.Infrastructure.Repositories;
using ECommerce.Product.Infrastructure.Services;
using ECommerce.Shared.Abstractions.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Product.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // HttpContextAccessor for CurrentUserService
            builder.Services.AddHttpContextAccessor();

            // Database with Audit Interceptor
            builder.Services.AddDbContext<ProductDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                
                // Add audit interceptor for automatic CreatedBy/UpdatedBy population
                var currentUserService = serviceProvider.GetService<ICurrentUserService>();
                options.AddInterceptors(new AuditableEntityInterceptor(currentUserService));
            });

            // Current User Service (for audit interceptor)
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Unit of Work & Repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
