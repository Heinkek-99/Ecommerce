using Ecommerce.Notifications.Domain;
using Ecommerce.Notifications.Infrastructure;
using Ecommerce.Shared.Events;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnOrderConfirmedNotify
{
    private readonly NotificationsDbContext _db;

    public OnOrderConfirmedNotify(NotificationsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(OrderConfirmedEvent evt, CancellationToken ct)
    {
        var payload = $"{{\"order_id\": \"{evt.OrderId}\", \"total\": \"{evt.TotalAmount} {evt.CurrencyCode}\"}}";

        var notification = Notification.Create(
            evt.UserId,
            channel: "email",
            payload: payload,
            orderId: evt.OrderId);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
    }
}
