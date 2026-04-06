using Ecommerce.Identity.Infrastructure;
<<<<<<< HEAD
<<<<<<< HEAD
using Ecommerce.Shared.Abstractions;
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace Ecommerce.Identity.Extensions;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        string connectionString)
    {
        // AddDbContextWithWolverineIntegration enrôle le DbContext dans la gestion
        // de transaction Wolverine → bus.PublishAsync() AVANT SaveChangesAsync()
        // écrit le message dans wolverine_outgoing_envelopes dans la MÊME transaction
        services.AddDbContextWithWolverineIntegration<IdentityDbContext>(opts =>
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

<<<<<<< HEAD
<<<<<<< HEAD
        services.AddScoped<IIdentityIntegrationService, IdentityIntegrationService>();

=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
        return services;
    }
}
