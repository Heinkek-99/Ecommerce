using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.Notifications.Infrastructure;

public class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new NotificationsDbContext(opts);
    }
}
