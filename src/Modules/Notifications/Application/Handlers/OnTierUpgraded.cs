using Ecommerce.Notifications.Domain;
using Ecommerce.Notifications.Infrastructure;
using Ecommerce.Shared.Events;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnTierUpgradedNotify
{
    private readonly NotificationsDbContext _db;

    public OnTierUpgradedNotify(NotificationsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(TierUpgradedEvent evt, CancellationToken ct)
    {
        var payload = $"{{\"old_tier\": \"{evt.OldTier}\", \"new_tier\": \"{evt.NewTier}\"}}";

        var notification = Notification.Create(
            evt.UserId,
            channel: "email",
            payload: payload);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
    }
}
