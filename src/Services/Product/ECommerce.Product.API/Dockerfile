# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Product/ECommerce.Product.API/ECommerce.Product.API.csproj", "Services/Product/ECommerce.Product.API/"]
COPY ["src/Services/Product/ECommerce.Product.Application/ECommerce.Product.Application.csproj", "Services/Product/ECommerce.Product.Application/"]
COPY ["src/Services/Product/ECommerce.Product.Domain/ECommerce.Product.Domain.csproj", "Services/Product/ECommerce.Product.Domain/"]
COPY ["src/Services/Product/ECommerce.Product.Infrastructure/ECommerce.Product.Infrastructure.csproj", "Services/Product/ECommerce.Product.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]

RUN dotnet restore "Services/Product/ECommerce.Product.API/ECommerce.Product.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/Product/ECommerce.Product.API"
RUN dotnet build "ECommerce.Product.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.Product.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Product.API.dll"]
