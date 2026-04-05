namespace Ecommerce.Promotions.Domain;

public class PromotionUse
{
    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; }

    private PromotionUse() { }

    public static PromotionUse Create(
        Guid promotionId,
        Guid userId,
        Guid orderId,
        decimal discountApplied)
    {
        return new PromotionUse
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            UserId = userId,
            OrderId = orderId,
            DiscountApplied = discountApplied,
            UsedAt = DateTime.UtcNow
        };
    }
}
