using Ecommerce.Notifications.Application.Commands.SendNotification;
using Ecommerce.Shared.Events;
using Wolverine;

namespace Ecommerce.Notifications.Application.Handlers;

public class OnTierUpgradedNotify
{
    public async Task Handle(TierUpgradedEvent evt, IMessageBus bus, CancellationToken ct)
    {
        await bus.InvokeAsync<Ecommerce.Shared.Common.Result>(
            new SendNotificationCommand(
                UserId: evt.UserId,
                Channel: "email",
                TemplateCode: "tier_upgraded",
                Variables: new Dictionary<string, string>
                {
                    ["old_tier"]  = evt.OldTier,
                    ["new_tier"]  = evt.NewTier,
                    ["user_name"] = "Client"
                }),
            ct);
    }
}
