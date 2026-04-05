namespace Ecommerce.Notifications.Domain;

public class UserNotificationPreference
{
    public Guid UserId { get; private set; }
    public string Channel { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public bool Subscribed { get; private set; }

    private UserNotificationPreference() { }
}
