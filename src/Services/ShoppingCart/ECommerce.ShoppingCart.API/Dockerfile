# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/ShoppingCart/ECommerce.ShoppingCart.API/ECommerce.ShoppingCart.API.csproj", "Services/ShoppingCart/ECommerce.ShoppingCart.API/"]
COPY ["src/Services/ShoppingCart/ECommerce.ShoppingCart.Application/ECommerce.ShoppingCart.Application.csproj", "Services/ShoppingCart/ECommerce.ShoppingCart.Application/"]
COPY ["src/Services/ShoppingCart/ECommerce.ShoppingCart.Domain/ECommerce.ShoppingCart.Domain.csproj", "Services/ShoppingCart/ECommerce.ShoppingCart.Domain/"]
COPY ["src/Services/ShoppingCart/ECommerce.ShoppingCart.Infrastructure/ECommerce.ShoppingCart.Infrastructure.csproj", "Services/ShoppingCart/ECommerce.ShoppingCart.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]

RUN dotnet restore "Services/ShoppingCart/ECommerce.ShoppingCart.API/ECommerce.ShoppingCart.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/ShoppingCart/ECommerce.ShoppingCart.API"
RUN dotnet build "ECommerce.ShoppingCart.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.ShoppingCart.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.ShoppingCart.API.dll"]
