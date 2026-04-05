using Ecommerce.Notifications.Domain;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Notifications.Infrastructure;

/// <summary>
/// Envoi email via MailKit. Stub pour dev — configure SMTP via appsettings.
/// </summary>
public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        // TODO: Intégration MailKit avec SMTP réel
        // Pour l'instant : log uniquement (dev/test)
        _logger.LogInformation(
            "Email notification {Id} envoyée à user {UserId} — payload: {Payload}",
            notification.Id,
            notification.UserId,
            notification.Payload);

        return Task.CompletedTask;
    }
}
