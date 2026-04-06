namespace Ecommerce.Promotions.Domain;

public class Promotion
{
    public Guid Id { get; private set; }
    public Guid? CampaignId { get; private set; }
    public string? Code { get; private set; }
    public string DiscountType { get; private set; } = default!;
    public decimal DiscountValue { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public int? MaxUses { get; private set; }
    public int UsesCount { get; private set; }
    public int MaxUsesPerUser { get; private set; } = 1;
    public bool Stackable { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PromotionUse> _uses = new();
    public IReadOnlyList<PromotionUse> Uses => _uses.AsReadOnly();

    private Promotion() { }

    public static Promotion Create(
        string discountType,
        decimal discountValue,
        string? code = null,
        Guid? campaignId = null,
        decimal minOrderAmount = 0,
        int? maxUses = null,
        DateTime? expiresAt = null)
    {
        return new Promotion
        {
            Id = Guid.NewGuid(),
            Code = code,
            CampaignId = campaignId,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MinOrderAmount = minOrderAmount,
            MaxUses = maxUses,
            UsesCount = 0,
            MaxUsesPerUser = 1,
            Stackable = false,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordUse(Guid userId, Guid orderId, decimal discountApplied = 0)
    {
        UsesCount++;
        _uses.Add(PromotionUse.Create(Id, userId, orderId, discountApplied));
    }
}
