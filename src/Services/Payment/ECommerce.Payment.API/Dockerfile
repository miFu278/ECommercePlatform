# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Payment/ECommerce.Payment.API/ECommerce.Payment.API.csproj", "Services/Payment/ECommerce.Payment.API/"]
COPY ["src/Services/Payment/ECommerce.Payment.Application/ECommerce.Payment.Application.csproj", "Services/Payment/ECommerce.Payment.Application/"]
COPY ["src/Services/Payment/ECommerce.Payment.Domain/ECommerce.Payment.Domain.csproj", "Services/Payment/ECommerce.Payment.Domain/"]
COPY ["src/Services/Payment/ECommerce.Payment.Infrastructure/ECommerce.Payment.Infrastructure.csproj", "Services/Payment/ECommerce.Payment.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]
COPY ["src/BuildingBlocks/ECommerce.EventBus/ECommerce.EventBus.csproj", "BuildingBlocks/ECommerce.EventBus/"]

RUN dotnet restore "Services/Payment/ECommerce.Payment.API/ECommerce.Payment.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/Payment/ECommerce.Payment.API"
RUN dotnet build "ECommerce.Payment.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.Payment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Payment.API.dll"]
