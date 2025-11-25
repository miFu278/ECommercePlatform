# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/Users/ECommerce.User.API/ECommerce.User.API.csproj", "Services/Users/ECommerce.User.API/"]
COPY ["src/Services/Users/ECommerce.User.Application/ECommerce.User.Application.csproj", "Services/Users/ECommerce.User.Application/"]
COPY ["src/Services/Users/ECommerce.User.Domain/ECommerce.User.Domain.csproj", "Services/Users/ECommerce.User.Domain/"]
COPY ["src/Services/Users/ECommerce.User.Infrastructure/ECommerce.User.Infrastructure.csproj", "Services/Users/ECommerce.User.Infrastructure/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]
COPY ["src/BuildingBlocks/ECommerce.Common/ECommerce.Common.csproj", "BuildingBlocks/ECommerce.Common/"]

RUN dotnet restore "Services/Users/ECommerce.User.API/ECommerce.User.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Services/Users/ECommerce.User.API"
RUN dotnet build "ECommerce.User.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.User.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.User.API.dll"]
