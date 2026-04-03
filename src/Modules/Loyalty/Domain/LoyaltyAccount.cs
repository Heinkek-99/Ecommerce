namespace Ecommerce.Loyalty.Domain;

public class LoyaltyAccount
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid? CurrentTierId { get; private set; }
    public int PointsBalance { get; private set; }
    public int PointsLifetime { get; private set; }
    public DateTime? TierUpdatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public LoyaltyTier? CurrentTier { get; private set; }

    private readonly List<LoyaltyTransaction> _transactions = new();
    public IReadOnlyList<LoyaltyTransaction> Transactions => _transactions.AsReadOnly();

    private LoyaltyAccount() { }

    public static LoyaltyAccount Create(Guid userId, Guid programId)
    {
        return new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProgramId = programId,
            PointsBalance = 0,
            PointsLifetime = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddPoints(int points, Guid? orderId = null, string? description = null)
    {
        if (points <= 0) throw new ArgumentException("Points must be positive.", nameof(points));

        PointsBalance += points;
        PointsLifetime += points;

        _transactions.Add(LoyaltyTransaction.Create(
            Id, "earn", points, orderId, description: description));
    }

    public void DeductPoints(int points, Guid rewardId, string? description = null)
    {
        if (PointsBalance < points)
            throw new InvalidOperationException("Insufficient points balance.");

        PointsBalance -= points;
        _transactions.Add(LoyaltyTransaction.Create(
            Id, "redeem", -points, rewardId: rewardId, description: description));
    }

    public void UpgradeTier(Guid tierId, string tierName)
    {
        CurrentTierId = tierId;
        TierUpdatedAt = DateTime.UtcNow;
    }
}
