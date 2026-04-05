using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Identity.Infrastructure;
using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Notifications.Infrastructure;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Promotions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Extensions;

public static class ModuleExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        await MigrateAsync<IdentityDbContext>(scope);
        await MigrateAsync<CatalogDbContext>(scope);
        await MigrateAsync<OrdersDbContext>(scope);
        await MigrateAsync<PromotionsDbContext>(scope);
        await MigrateAsync<LoyaltyDbContext>(scope);
        await MigrateAsync<NotificationsDbContext>(scope);
    }

    private static async Task MigrateAsync<TContext>(IServiceScope scope)
        where TContext : DbContext
    {
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.MigrateAsync();
    }
}
