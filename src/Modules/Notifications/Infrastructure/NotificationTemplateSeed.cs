using Ecommerce.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Notifications.Infrastructure;

public static class NotificationTemplateSeed
{
    private static readonly (string EventType, string Subject, string Body)[] Templates =
    [
        (
            "order_confirmed",
            "Commande {{order_id}} confirmée",
            "Bonjour {{user_name}}, votre commande de {{total}} {{currency}} est confirmée."
        ),
        (
            "order_shipped",
            "Commande {{order_id}} expédiée",
            "Votre commande a été expédiée. Tracking : {{tracking_number}}"
        ),
        (
            "tier_upgraded",
            "Félicitations ! Vous êtes maintenant {{new_tier}}",
            "{{user_name}}, vous passez de {{old_tier}} à {{new_tier}}."
        ),
        (
            "new_order_seller",
            "Nouvelle commande reçue #{{order_id}}",
            "Vous avez reçu une nouvelle commande de {{total}} {{currency}}."
        )
    ];

    public static async Task SeedAsync(NotificationsDbContext db)
    {
        foreach (var (eventType, subject, body) in Templates)
        {
            var exists = await db.NotificationTemplates
                .AnyAsync(t => t.EventType == eventType && t.Channel == "email");

            if (!exists)
            {
                db.NotificationTemplates.Add(
                    NotificationTemplate.Create(eventType, "email", body, subject));
            }
        }

        await db.SaveChangesAsync();
    }
}
