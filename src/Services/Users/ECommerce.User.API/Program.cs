using System.Text;
using ECommerce.User.API.Middleware;
using ECommerce.User.Application;
using ECommerce.User.Application.Interfaces;
using ECommerce.User.Application.Mappings;
using ECommerce.User.Application.Services;
using ECommerce.User.Application.Validators;
using ECommerce.User.Domain.Interfaces;
using ECommerce.User.Infrastructure.Data;
using ECommerce.User.Infrastructure.Repositories;
using ECommerce.User.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Fix PostgreSQL DateTime issue - treat DateTimeKind.Unspecified as UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerce User Service API",
        Version = "v1",
        Description = "API for user authentication and profile management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ECommerce Platform",
            Email = "support@ecommerce.com"
        }
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Database with Audit Interceptor
builder.Services.AddDbContext<UserDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // Add audit interceptor for automatic CreatedBy/UpdatedBy population
    var currentUserService = serviceProvider.GetService<ECommerce.Shared.Abstractions.Interceptors.ICurrentUserService>();
    options.AddInterceptors(new ECommerce.Shared.Abstractions.Interceptors.AuditableEntityInterceptor(currentUserService));
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// Current User Service (for audit interceptor)
builder.Services.AddScoped<ECommerce.Shared.Abstractions.Interceptors.ICurrentUserService, CurrentUserService>();

// Unit of Work & Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce User Service API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at app root
        options.DocumentTitle = "ECommerce User Service API";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
