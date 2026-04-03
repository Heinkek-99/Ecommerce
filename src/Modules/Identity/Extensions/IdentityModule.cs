using Ecommerce.Identity.Infrastructure;
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

        return services;
    }
}
