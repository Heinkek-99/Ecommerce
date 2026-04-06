using Ecommerce.Notifications.Application.Jobs;

namespace Ecommerce.Api.BackgroundServices;

public class NotificationsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationsBackgroundService> _logger;

    public NotificationsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<ProcessPendingNotificationsJob>();
                await job.Execute(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPendingNotificationsJob");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}
