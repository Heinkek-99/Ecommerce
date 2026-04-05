namespace Ecommerce.Promotions.Domain;

public class FlashSale
{
    public Guid Id { get; private set; }
    public Guid? CampaignId { get; private set; }
    public Guid VariantId { get; private set; }
    public decimal SalePrice { get; private set; }
    public int? StockLimit { get; private set; }
    public int SoldCount { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    public bool IsActive =>
        DateTime.UtcNow >= StartsAt &&
        DateTime.UtcNow <= EndsAt &&
        (StockLimit is null || SoldCount < StockLimit);

    private FlashSale() { }

    public static FlashSale Create(
        Guid variantId,
        decimal salePrice,
        DateTime startsAt,
        DateTime endsAt,
        Guid? campaignId = null,
        int? stockLimit = null)
    {
        return new FlashSale
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            SalePrice = salePrice,
            StartsAt = startsAt,
            EndsAt = endsAt,
            CampaignId = campaignId,
            StockLimit = stockLimit,
            SoldCount = 0
        };
    }

    public void IncrementSold()
    {
        if (StockLimit.HasValue && SoldCount >= StockLimit.Value)
            throw new InvalidOperationException("Flash sale stock exhausted.");
        SoldCount++;
    }
}
