namespace Ecommerce.Loyalty.Domain;

public class LoyaltyTier
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public string Name { get; private set; } = default!;
    public int MinPoints { get; private set; }
    public decimal BonusMultiplier { get; private set; }
    public string Perks { get; private set; } = "{}";
    public int Rank { get; private set; }

    private LoyaltyTier() { }

    public static LoyaltyTier Create(
        Guid programId,
        string name,
        int minPoints,
        decimal bonusMultiplier,
        int rank,
        string perks = "{}")
    {
        return new LoyaltyTier
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            Name = name,
            MinPoints = minPoints,
            BonusMultiplier = bonusMultiplier,
            Rank = rank,
            Perks = perks
        };
    }
}
