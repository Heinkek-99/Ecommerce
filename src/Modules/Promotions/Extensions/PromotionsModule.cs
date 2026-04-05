using Ecommerce.Promotions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Promotions.Extensions;

public static class PromotionsModule
{
    public static IServiceCollection AddPromotionsModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PromotionsDbContext>(opts =>
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        return services;
    }
}
