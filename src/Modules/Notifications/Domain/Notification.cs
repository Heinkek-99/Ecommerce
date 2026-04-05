namespace Ecommerce.Notifications.Domain;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? TemplateId { get; private set; }
    public Guid? OrderId { get; private set; }
    public string Channel { get; private set; } = default!;
    public string Status { get; private set; } = "pending";
    public string Payload { get; private set; } = "{}";
    public DateTime ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public NotificationTemplate? Template { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        string channel,
        string payload,
        Guid? templateId = null,
        Guid? orderId = null,
        DateTime? scheduledAt = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = channel,
            Payload = payload,
            TemplateId = templateId,
            OrderId = orderId,
            Status = "pending",
            ScheduledAt = scheduledAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkSent()
    {
        Status = "sent";
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = "failed";
    public void MarkSending() => Status = "sending";
}
