using Ecommerce.Loyalty.Domain;
using FluentAssertions;

namespace Ecommerce.Loyalty.Tests.Application;

/// <summary>
/// Tests domain logic exercisée par EarnPointsHandler.
/// </summary>
public class EarnPointsHandlerTests
{
    private static (LoyaltyAccount, LoyaltyProgram) MakeAccountAndProgram(decimal pointsPerEuro = 10m)
    {
        var program = LoyaltyProgram.Create("Test Program", pointsPerEuro);
        var account = LoyaltyAccount.Create(Guid.NewGuid(), program.Id);
        return (account, program);
    }

    [Fact]
    public void Should_earn_points_with_correct_multiplier()
    {
        var (account, program) = MakeAccountAndProgram(pointsPerEuro: 10m);
        var multiplier = 1.5m; // Silver tier
        var orderAmount = 50m;

        var points = (int)Math.Floor(orderAmount * program.PointsPerEuro * multiplier);
        account.AddPoints(points, Guid.NewGuid());

        account.PointsBalance.Should().Be(750);   // 50 × 10 × 1.5
        account.PointsLifetime.Should().Be(750);
    }

    [Fact]
    public void Should_be_idempotent_when_same_orderId()
    {
        var (account, _) = MakeAccountAndProgram();
        var orderId = Guid.NewGuid();

        // Premier appel
        var alreadyDone = account.Transactions.Any(t => t.OrderId == orderId && t.Type == "earn");
        if (!alreadyDone) account.AddPoints(100, orderId);

        // Deuxième appel (doublon)
        alreadyDone = account.Transactions.Any(t => t.OrderId == orderId && t.Type == "earn");
        if (!alreadyDone) account.AddPoints(100, orderId);

        account.Transactions.Should().HaveCount(1);
        account.PointsBalance.Should().Be(100);
    }

    [Fact]
    public void Should_upgrade_tier_when_threshold_reached()
    {
        var (account, program) = MakeAccountAndProgram();
        var silverTier = LoyaltyTier.Create(program.Id, "Silver", minPoints: 500, bonusMultiplier: 1.5m, rank: 2);

        // Ajouter assez de points pour atteindre Silver
        account.AddPoints(600, Guid.NewGuid());

        // Simuler la vérification du handler
        var qualifies = account.PointsLifetime >= silverTier.MinPoints;
        if (qualifies)
            account.UpgradeTier(silverTier.Id, silverTier.Name);

        qualifies.Should().BeTrue();
        account.CurrentTierId.Should().Be(silverTier.Id);
    }
}
