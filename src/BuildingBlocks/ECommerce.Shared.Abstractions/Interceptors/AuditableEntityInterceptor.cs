using ECommerce.Shared.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ECommerce.Shared.Abstractions.Interceptors;

/// <summary>
/// EF Core interceptor to automatically populate audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService? _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService? currentUserService = null)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        var currentUserId = _currentUserService?.GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Handle BaseEntity (CreatedAt, UpdatedAt)
            if (entry.Entity is BaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = now;
                        baseEntity.UpdatedAt = now;
                        break;
                    case EntityState.Modified:
                        baseEntity.UpdatedAt = now;
                        break;
                }
            }

            // Handle IAuditableEntity (CreatedBy, UpdatedBy)
            if (entry.Entity is IAuditableEntity auditableEntity && currentUserId.HasValue)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedBy = currentUserId.Value;
                        auditableEntity.UpdatedBy = currentUserId.Value;
                        break;
                    case EntityState.Modified:
                        auditableEntity.UpdatedBy = currentUserId.Value;
                        break;
                }
            }

            // Handle ISoftDeletable
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.DeletedAt = now;
            }
        }
    }
}

/// <summary>
/// Service to get current authenticated user ID
/// Implement this in each service's Infrastructure layer
/// </summary>
public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
}
