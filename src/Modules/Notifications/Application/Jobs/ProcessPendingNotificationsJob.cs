using Ecommerce.Notifications.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Notifications.Application.Jobs;

/// <summary>
/// Job récurrent — Wolverine Scheduler (toutes les 1 minute).
/// Lit la table notifications avec status='pending' et envoie via EmailSender/SmsSender.
/// </summary>
public class ProcessPendingNotificationsJob
{
    private readonly NotificationsDbContext _db;
    private readonly EmailSender _emailSender;

    public ProcessPendingNotificationsJob(NotificationsDbContext db, EmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    public async Task Execute(CancellationToken ct)
    {
        var pending = await _db.Notifications
            .Include(n => n.Template)
            .Where(n => n.Status == "pending" && n.ScheduledAt <= DateTime.UtcNow)
            .Take(50)
            .ToListAsync(ct);

        foreach (var notification in pending)
        {
            notification.MarkSending();

            try
            {
                if (notification.Channel == "email")
                    await _emailSender.SendAsync(notification, ct);

                notification.MarkSent();
            }
            catch
            {
                notification.MarkFailed();
            }
        }

        if (pending.Any())
            await _db.SaveChangesAsync(ct);
    }
}
