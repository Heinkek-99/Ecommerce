using Ecommerce.Catalog.Infrastructure;
<<<<<<< HEAD
using Ecommerce.Shared.Abstractions;
=======
>>>>>>> feature/orders-logic
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Catalog.Extensions;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(opts =>
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

<<<<<<< HEAD
        services.AddScoped<ICatalogIntegrationService, CatalogIntegrationService>();

=======
>>>>>>> feature/orders-logic
        return services;
    }
}
