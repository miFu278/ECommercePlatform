using ECommerce.Payment.Domain.Enums;
using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.Payment.Domain.Entities;

public class PaymentHistory : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    
    public PaymentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    
    // Navigation
    public PaymentEntity Payment { get; set; } = null!;
}
