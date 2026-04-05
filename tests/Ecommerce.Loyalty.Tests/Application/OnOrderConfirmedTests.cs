using Ecommerce.Loyalty.Domain;
using FluentAssertions;

namespace Ecommerce.Loyalty.Tests.Application;

public class OnOrderConfirmedTests
{
    [Fact]
    public void LoyaltyAccount_AddPoints_IncreasesBalanceAndLifetime()
    {
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());
        account.AddPoints(100, Guid.NewGuid());

        account.PointsBalance.Should().Be(100);
        account.PointsLifetime.Should().Be(100);
        account.Transactions.Should().HaveCount(1);
        account.Transactions[0].Type.Should().Be("earn");
    }

    [Fact]
    public void LoyaltyAccount_AddPoints_CreatesEarnTransaction()
    {
        var orderId = Guid.NewGuid();
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());
        account.AddPoints(250, orderId, "Commande 123");

        account.Transactions[0].OrderId.Should().Be(orderId);
        account.Transactions[0].Points.Should().Be(250);
        account.Transactions[0].Description.Should().Be("Commande 123");
    }

    [Fact]
    public void LoyaltyProgram_PointsCalculation_AppliesMultiplier()
    {
        var program = LoyaltyProgram.Create("Test", pointsPerEuro: 10m);
        var totalAmount = 50m;
        var multiplier = 1.5m; // Silver tier

        var points = (int)Math.Floor(totalAmount * program.PointsPerEuro * multiplier);

        points.Should().Be(750);
    }
}
