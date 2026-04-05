using Ecommerce.Notifications.Domain;
using Ecommerce.Notifications.Infrastructure;
using Ecommerce.Shared.Events;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnOrderShippedNotify
{
    private readonly NotificationsDbContext _db;

    public OnOrderShippedNotify(NotificationsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(OrderShippedEvent evt, CancellationToken ct)
    {
        var payload = $"{{\"order_id\": \"{evt.OrderId}\", \"tracking\": \"{evt.TrackingNumber}\"}}";

        var notification = Notification.Create(
            evt.UserId,
            channel: "email",
            payload: payload,
            orderId: evt.OrderId);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
    }
}
