namespace Ecommerce.Loyalty.Domain;

public class LoyaltyProgram
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal PointsPerEuro { get; private set; }
    public decimal EuroPerPoint { get; private set; }
    public int? ExpiryDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LoyaltyProgram() { }

    public static LoyaltyProgram Create(
        string name,
        decimal pointsPerEuro = 10m,
        decimal euroPerPoint = 0.01m,
        int? expiryDays = null)
    {
        return new LoyaltyProgram
        {
            Id = Guid.NewGuid(),
            Name = name,
            PointsPerEuro = pointsPerEuro,
            EuroPerPoint = euroPerPoint,
            ExpiryDays = expiryDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
