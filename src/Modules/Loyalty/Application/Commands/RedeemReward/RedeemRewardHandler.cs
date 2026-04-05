using Ecommerce.Loyalty.Domain;
using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Application.Commands.RedeemReward;

public class RedeemRewardHandler
{
    private readonly LoyaltyDbContext _db;

    public RedeemRewardHandler(LoyaltyDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(RedeemRewardCommand cmd, CancellationToken ct)
    {
        var account = await _db.LoyaltyAccounts
            .FirstOrDefaultAsync(a => a.UserId == cmd.UserId, ct);
        if (account is null)
            return Result.Failure<Guid>("Loyalty account not found.");

        var reward = await _db.LoyaltyRewards
            .FirstOrDefaultAsync(r => r.Id == cmd.RewardId && r.IsActive, ct);
        if (reward is null)
            return Result.Failure<Guid>("Reward not found or inactive.");

        if (account.PointsBalance < reward.PointsCost)
            return Result.Failure<Guid>($"Insufficient points. Required: {reward.PointsCost}, available: {account.PointsBalance}.");

        if (reward.Stock.HasValue && reward.Stock <= 0)
            return Result.Failure<Guid>("Reward is out of stock.");

        // Décrémente points_balance et reward stock atomiquement
        account.DeductPoints(reward.PointsCost, reward.Id, reward.Name);
        reward.DecrementStock();

        var redemption = LoyaltyRedemption.Create(account.Id, reward.Id, reward.PointsCost);
        _db.LoyaltyRedemptions.Add(redemption);

        await _db.SaveChangesAsync(ct);
        return Result.Success(redemption.Id);
    }
}
