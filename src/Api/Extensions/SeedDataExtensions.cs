using Ecommerce.Catalog.Domain;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Loyalty.Domain;
using Ecommerce.Loyalty.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedDevelopmentDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await SeedCategoriesAsync(sp.GetRequiredService<CatalogDbContext>());
        await SeedLoyaltyAsync(sp.GetRequiredService<LoyaltyDbContext>());
    }

    private static async Task SeedCategoriesAsync(CatalogDbContext db)
    {
        if (await db.Categories.AnyAsync())
            return;

        var parents = new[]
        {
            Category.Create("Mode & Accessoires",  "mode-accessoires"),
            Category.Create("Électronique",        "electronique"),
            Category.Create("Maison & Artisanat",  "maison-artisanat"),
        };
        db.Categories.AddRange(parents);
        await db.SaveChangesAsync();

        var children = new[]
        {
            // Mode
            Category.Create("Vêtements",      "vetements",      parents[0].Id),
            Category.Create("Chaussures",     "chaussures",     parents[0].Id),
            Category.Create("Bijoux",         "bijoux",         parents[0].Id),
            // Électronique
            Category.Create("Smartphones",   "smartphones",    parents[1].Id),
            Category.Create("Ordinateurs",   "ordinateurs",    parents[1].Id),
            Category.Create("Audio & TV",    "audio-tv",       parents[1].Id),
            // Maison
            Category.Create("Décoration",    "decoration",     parents[2].Id),
            Category.Create("Cuisine",       "cuisine",        parents[2].Id),
            Category.Create("Artisanat",     "artisanat",      parents[2].Id),
        };
        db.Categories.AddRange(children);
        await db.SaveChangesAsync();
    }

    private static async Task SeedLoyaltyAsync(LoyaltyDbContext db)
    {
        if (await db.LoyaltyPrograms.AnyAsync())
            return;

        var program = LoyaltyProgram.Create("GlobalMart Fidélité", pointsPerEuro: 1m);
        db.LoyaltyPrograms.Add(program);
        await db.SaveChangesAsync();

        var tiers = new[]
        {
            LoyaltyTier.Create(program.Id, "Bronze",   minPoints:    0, bonusMultiplier: 1.0m, rank: 1),
            LoyaltyTier.Create(program.Id, "Silver",   minPoints:  500, bonusMultiplier: 1.5m, rank: 2),
            LoyaltyTier.Create(program.Id, "Gold",     minPoints: 2000, bonusMultiplier: 2.0m, rank: 3),
            LoyaltyTier.Create(program.Id, "Platinum", minPoints: 5000, bonusMultiplier: 3.0m, rank: 4),
        };
        db.LoyaltyTiers.AddRange(tiers);
        await db.SaveChangesAsync();
    }
}
