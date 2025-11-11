# BuildingBlocks

Shared components and abstractions used across all microservices.

## Structure

```
BuildingBlocks/
├── ECommerce.Common/              # Common utilities, exceptions, models
├── ECommerce.EventBus/            # Event-driven communication abstractions
├── ECommerce.Logging/             # Logging extensions
└── ECommerce.Shared.Abstractions/ # Domain & Repository interfaces (NEW)
```

## ECommerce.Shared.Abstractions

**Philosophy:** Pure abstractions only - no implementations or dependencies.

Contains:
- `IRepository<T>` - Generic repository interface
- `IUnitOfWork` - Unit of Work interface
- `BaseEntity`, `IAuditableEntity`, `ISoftDeletable` - Entity abstractions

Each microservice implements these interfaces independently in their Infrastructure layer.

## Design Principles

### ✅ What belongs in BuildingBlocks:
- Interfaces and abstractions
- Common exceptions and models
- Shared utilities (extensions, constants)
- Event contracts for inter-service communication

### ❌ What does NOT belong here:
- Concrete implementations with external dependencies
- Business logic
- Service-specific code
- Database-specific implementations

## Usage

Services reference only what they need:

```xml
<!-- In Product.Domain.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\BuildingBlocks\ECommerce.Shared.Abstractions\ECommerce.Shared.Abstractions.csproj" />
  <ProjectReference Include="..\..\BuildingBlocks\ECommerce.Common\ECommerce.Common.csproj" />
</ItemGroup>
```

## Benefits

✅ **DRY** - Shared code in one place
✅ **Consistency** - Same patterns across services
✅ **Autonomy** - Services remain independent
✅ **Flexibility** - Services can customize implementations
