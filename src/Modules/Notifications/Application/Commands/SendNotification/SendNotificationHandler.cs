using System.Text.Json;
using Ecommerce.Notifications.Domain;
using Ecommerce.Notifications.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Notifications.Application.Commands.SendNotification;

public class SendNotificationHandler
{
    private readonly NotificationsDbContext _db;

    public SendNotificationHandler(NotificationsDbContext db) => _db = db;

    public async Task<Result> Handle(SendNotificationCommand cmd, CancellationToken ct)
    {
        // Charge le template par code (event_type) et channel
        var template = await _db.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.EventType == cmd.TemplateCode &&
                t.Channel == cmd.Channel &&
                t.IsActive, ct);

        string payload;
        if (template is not null)
        {
            var subject = ReplaceVariables(template.Subject ?? string.Empty, cmd.Variables);
            var body = ReplaceVariables(template.BodyTemplate, cmd.Variables);
            payload = JsonSerializer.Serialize(new { subject, body });
        }
        else
        {
            // Fallback : payload JSON brut avec les variables
            payload = JsonSerializer.Serialize(new
            {
                template_code = cmd.TemplateCode,
                variables = cmd.Variables
            });
        }

        var notification = Notification.Create(
            cmd.UserId,
            cmd.Channel,
            payload,
            templateId: template?.Id,
            orderId: cmd.OrderId);

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> vars)
    {
        foreach (var (key, value) in vars)
            template = template.Replace($"{{{{{key}}}}}", value);
        return template;
    }
}
