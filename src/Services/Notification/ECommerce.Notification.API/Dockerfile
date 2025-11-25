# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Notification/ECommerce.Notification.API/ECommerce.Notification.API.csproj", "Services/Notification/ECommerce.Notification.API/"]
COPY ["src/Services/Notification/ECommerce.Notification.Application/ECommerce.Notification.Application.csproj", "Services/Notification/ECommerce.Notification.Application/"]
COPY ["src/Services/Notification/ECommerce.Notification.Domain/ECommerce.Notification.Domain.csproj", "Services/Notification/ECommerce.Notification.Domain/"]
COPY ["src/Services/Notification/ECommerce.Notification.Infrastructure/ECommerce.Notification.Infrastructure.csproj", "Services/Notification/ECommerce.Notification.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]
COPY ["src/BuildingBlocks/ECommerce.EventBus/ECommerce.EventBus.csproj", "BuildingBlocks/ECommerce.EventBus/"]

RUN dotnet restore "Services/Notification/ECommerce.Notification.API/ECommerce.Notification.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/Notification/ECommerce.Notification.API"
RUN dotnet build "ECommerce.Notification.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Notification.API.dll"]
