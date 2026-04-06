using Ecommerce.Notifications.Domain;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Notifications.Infrastructure;

public class SmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        // TODO: Intégration Twilio
        _logger.LogInformation(
            "SMS notification {Id} envoyée à user {UserId}",
            notification.Id,
            notification.UserId);

        return Task.CompletedTask;
    }
}
