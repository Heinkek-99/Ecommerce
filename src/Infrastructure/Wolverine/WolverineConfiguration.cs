using JasperFx.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;

namespace Ecommerce.Infrastructure.Wolverine;

public static class WolverineConfiguration
{
    public static WolverineOptions AddWolverineConfiguration(
        this WolverineOptions opts,
        string connectionString)
    {
        opts.PersistMessagesWithPostgresql(connectionString, schemaName: "wolverine");

        // Retry policy : 3 tentatives avec cooldown exponentiel
        opts.OnException<Exception>()
            .RetryWithCooldown(
                50.Milliseconds(),
                100.Milliseconds(),
                250.Milliseconds());

        return opts;
    }
}
