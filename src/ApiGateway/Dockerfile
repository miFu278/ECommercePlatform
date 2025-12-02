# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/ApiGateway/ECommerce.ApiGateway.csproj", "ApiGateway/"]
COPY ["src/BuildingBlocks/ECommerce.Shared.Abstractions/ECommerce.Shared.Abstractions.csproj", "BuildingBlocks/ECommerce.Shared.Abstractions/"]
COPY ["src/BuildingBlocks/ECommerce.Common/ECommerce.Common.csproj", "BuildingBlocks/ECommerce.Common/"]

RUN dotnet restore "ApiGateway/ECommerce.ApiGateway.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/ApiGateway"
RUN dotnet build "ECommerce.ApiGateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.ApiGateway.dll"]
