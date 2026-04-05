using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.Catalog.Infrastructure;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new CatalogDbContext(opts);
    }
}
