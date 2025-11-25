using ECommerce.Notification.Domain.Attributes;
using ECommerce.Notification.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Notification.Domain.Entities;

[BsonCollection("email_logs")]
public class EmailLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("notificationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? NotificationId { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("from")]
    public string From { get; set; } = string.Empty;

    [BsonElement("to")]
    public string To { get; set; } = string.Empty;

    [BsonElement("subject")]
    public string Subject { get; set; } = string.Empty;

    [BsonElement("bodyText")]
    public string BodyText { get; set; } = string.Empty;

    [BsonElement("bodyHtml")]
    public string BodyHtml { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public NotificationStatus Status { get; set; }

    [BsonElement("sentAt")]
    public DateTime? SentAt { get; set; }

    [BsonElement("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [BsonElement("openedAt")]
    public DateTime? OpenedAt { get; set; }

    [BsonElement("clickedAt")]
    public DateTime? ClickedAt { get; set; }

    [BsonElement("bouncedAt")]
    public DateTime? BouncedAt { get; set; }

    [BsonElement("openCount")]
    public int OpenCount { get; set; }

    [BsonElement("clickCount")]
    public int ClickCount { get; set; }

    [BsonElement("error")]
    public ErrorInfo? Error { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
