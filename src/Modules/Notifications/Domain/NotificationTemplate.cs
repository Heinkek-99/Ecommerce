namespace Ecommerce.Notifications.Domain;

public class NotificationTemplate
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = default!;
    public string Channel { get; private set; } = default!;
    public string Lang { get; private set; } = "fr";
    public string? Subject { get; private set; }
    public string BodyTemplate { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private NotificationTemplate() { }
}
