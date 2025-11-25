# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Order/ECommerce.Order.API/ECommerce.Order.API.csproj", "Services/Order/ECommerce.Order.API/"]
COPY ["src/Services/Order/ECommerce.Order.Application/ECommerce.Order.Application.csproj", "Services/Order/ECommerce.Order.Application/"]
COPY ["src/Services/Order/ECommerce.Order.Domain/ECommerce.Order.Domain.csproj", "Services/Order/ECommerce.Order.Domain/"]
COPY ["src/Services/Order/ECommerce.Order.Infrastructure/ECommerce.Order.Infrastructure.csproj", "Services/Order/ECommerce.Order.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]
COPY ["src/BuildingBlocks/ECommerce.EventBus/ECommerce.EventBus.csproj", "BuildingBlocks/ECommerce.EventBus/"]

RUN dotnet restore "Services/Order/ECommerce.Order.API/ECommerce.Order.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/Order/ECommerce.Order.API"
RUN dotnet build "ECommerce.Order.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.Order.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Order.API.dll"]
