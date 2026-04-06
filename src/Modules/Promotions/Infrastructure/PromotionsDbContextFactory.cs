using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.Promotions.Infrastructure;

public class PromotionsDbContextFactory : IDesignTimeDbContextFactory<PromotionsDbContext>
{
    public PromotionsDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<PromotionsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new PromotionsDbContext(opts);
    }
}
