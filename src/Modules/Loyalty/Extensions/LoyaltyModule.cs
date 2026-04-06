using Ecommerce.Loyalty.Application.Jobs;
using Ecommerce.Loyalty.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Loyalty.Extensions;

public static class LoyaltyModule
{
    public static IServiceCollection AddLoyaltyModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LoyaltyDbContext>(opts =>
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ExpireLoyaltyPointsJob>();

        return services;
    }
}
