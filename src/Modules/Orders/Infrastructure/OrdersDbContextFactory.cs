using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.Orders.Infrastructure;

public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new OrdersDbContext(opts);
    }
}
