using Ecommerce.Loyalty.Application.Jobs;

namespace Ecommerce.Api.BackgroundServices;

public class LoyaltyExpiryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoyaltyExpiryBackgroundService> _logger;

    public LoyaltyExpiryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LoyaltyExpiryBackgroundService> logger)
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
                var job = scope.ServiceProvider.GetRequiredService<ExpireLoyaltyPointsJob>();
                await job.Execute(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpireLoyaltyPointsJob");
            }

            // Exécution quotidienne
            await Task.Delay(TimeSpan.FromHours(24), ct);
        }
    }
}
