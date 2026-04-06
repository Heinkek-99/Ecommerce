using Ecommerce.Notifications.Application.Commands.SendNotification;
using Ecommerce.Shared.Events;
using Wolverine;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnOrderShippedNotify
{
    public async Task Handle(OrderShippedEvent evt, IMessageBus bus, CancellationToken ct)
    {
        await bus.InvokeAsync<Ecommerce.Shared.Common.Result>(
            new SendNotificationCommand(
                UserId: evt.UserId,
                Channel: "email",
                TemplateCode: "order_shipped",
                Variables: new Dictionary<string, string>
                {
                    ["order_id"]        = evt.OrderId.ToString(),
                    ["tracking_number"] = evt.TrackingNumber
                },
                OrderId: evt.OrderId),
            ct);
    }
}
