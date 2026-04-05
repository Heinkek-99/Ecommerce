using Ecommerce.Loyalty.Domain;
using FluentAssertions;

namespace Ecommerce.Loyalty.Tests.Application;

/// <summary>
/// Tests domain logic exercisée par RedeemRewardHandler.
/// </summary>
public class RedeemRewardHandlerTests
{
    private static LoyaltyAccount MakeAccountWithPoints(int balance)
    {
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());
        if (balance > 0)
            account.AddPoints(balance, Guid.NewGuid());
        return account;
    }

    private static LoyaltyReward MakeReward(int cost, int? stock = null)
        => LoyaltyReward.Create(Guid.NewGuid(), "Test Reward", "discount", cost, stock: stock);

    [Fact]
    public void Should_redeem_when_sufficient_points()
    {
        var account = MakeAccountWithPoints(500);
        var reward = MakeReward(cost: 200);

        account.DeductPoints(reward.PointsCost, reward.Id, reward.Name);

        account.PointsBalance.Should().Be(300);
        account.Transactions.Should().HaveCount(2); // earn + redeem
        account.Transactions[1].Type.Should().Be("redeem");
        account.Transactions[1].Points.Should().Be(-200);
    }

    [Fact]
    public void Should_fail_when_insufficient_points()
    {
        var account = MakeAccountWithPoints(100);
        var reward = MakeReward(cost: 500);

        var act = () => account.DeductPoints(reward.PointsCost, reward.Id);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient*");
    }

    [Fact]
    public void Should_fail_when_reward_out_of_stock()
    {
        var reward = MakeReward(cost: 100, stock: 0);

        var act = () => reward.DecrementStock();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*out of stock*");
    }
}
