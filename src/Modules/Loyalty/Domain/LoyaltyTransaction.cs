namespace Ecommerce.Loyalty.Domain;

public class LoyaltyTransaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid? RewardId { get; private set; }
    public string Type { get; private set; } = default!;
    public int Points { get; private set; }
    public string? Description { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LoyaltyTransaction() { }

    public static LoyaltyTransaction Create(
        Guid accountId,
        string type,
        int points,
        Guid? orderId = null,
        Guid? rewardId = null,
        string? description = null,
        DateTime? expiresAt = null)
    {
        return new LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            OrderId = orderId,
            RewardId = rewardId,
            Type = type,
            Points = points,
            Description = description,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }
}
