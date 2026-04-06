namespace Ecommerce.Loyalty.Domain;

public class LoyaltyRedemption
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid RewardId { get; private set; }
    public Guid? OrderId { get; private set; }
    public int PointsSpent { get; private set; }
    public string Status { get; private set; } = "pending";
    public DateTime RedeemedAt { get; private set; }

    private LoyaltyRedemption() { }

    public static LoyaltyRedemption Create(
        Guid accountId,
        Guid rewardId,
        int pointsSpent,
        Guid? orderId = null)
    {
        return new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            RewardId = rewardId,
            PointsSpent = pointsSpent,
            OrderId = orderId,
            Status = "applied",
            RedeemedAt = DateTime.UtcNow
        };
    }
}
