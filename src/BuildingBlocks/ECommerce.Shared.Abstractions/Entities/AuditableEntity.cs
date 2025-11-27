namespace ECommerce.Shared.Abstractions.Entities;

public abstract class AuditableEntity : IAuditableEntity
{
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
