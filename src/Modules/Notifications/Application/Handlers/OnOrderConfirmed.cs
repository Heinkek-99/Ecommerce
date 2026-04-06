using Ecommerce.Notifications.Application.Commands.SendNotification;
using Ecommerce.Shared.Events;
using Wolverine;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnOrderConfirmedNotify
{
    public async Task Handle(OrderConfirmedEvent evt, IMessageBus bus, CancellationToken ct)
    {
        await bus.InvokeAsync<Ecommerce.Shared.Common.Result>(
            new SendNotificationCommand(
                UserId: evt.UserId,
                Channel: "email",
                TemplateCode: "order_confirmed",
                Variables: new Dictionary<string, string>
                {
                    ["order_id"] = evt.OrderId.ToString(),
                    ["total"]    = evt.TotalAmount.ToString("F2"),
                    ["currency"] = evt.CurrencyCode,
                    ["user_name"] = "Client"
                },
                OrderId: evt.OrderId),
            ct);
    }
}
