using ECommerce.Notification.Domain.Attributes;
using ECommerce.Notification.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Notification.Domain.Entities;

[BsonCollection("notification_logs")]
public class NotificationLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public NotificationType Type { get; set; }

    [BsonElement("channel")]
    [BsonRepresentation(BsonType.String)]
    public NotificationChannel Channel { get; set; }

    [BsonElement("subject")]
    public string Subject { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("recipient")]
    public RecipientInfo Recipient { get; set; } = new();

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public NotificationStatus Status { get; set; }

    [BsonElement("attempts")]
    public int Attempts { get; set; }

    [BsonElement("maxAttempts")]
    public int MaxAttempts { get; set; } = 3;

    [BsonElement("sentAt")]
    public DateTime? SentAt { get; set; }

    [BsonElement("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [BsonElement("failedAt")]
    public DateTime? FailedAt { get; set; }

    [BsonElement("error")]
    public ErrorInfo? Error { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class RecipientInfo
{
    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }
}

public class ErrorInfo
{
    [BsonElement("code")]
    public string? Code { get; set; }

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("details")]
    public string? Details { get; set; }
}
