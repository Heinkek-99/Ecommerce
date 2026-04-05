namespace Ecommerce.Loyalty.Domain;

public class LoyaltyReward
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public string Name { get; private set; } = default!;
    public string RewardType { get; private set; } = default!;
    public int PointsCost { get; private set; }
    public decimal? DiscountValue { get; private set; }
    public int? Stock { get; private set; }
    public string Metadata { get; private set; } = "{}";
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LoyaltyReward() { }

    public void DecrementStock()
    {
        if (Stock is null) return;
        if (Stock <= 0) throw new InvalidOperationException("Reward is out of stock.");
        Stock--;
    }

    public static LoyaltyReward Create(
        Guid programId,
        string name,
        string rewardType,
        int pointsCost,
        decimal? discountValue = null,
        int? stock = null)
    {
        return new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            Name = name,
            RewardType = rewardType,
            PointsCost = pointsCost,
            DiscountValue = discountValue,
            Stock = stock,
            Metadata = "{}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
