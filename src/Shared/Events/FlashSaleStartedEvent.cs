namespace Ecommerce.Shared.Events;

public record FlashSaleStartedEvent(
    Guid FlashSaleId,
    Guid VariantId,
    decimal SalePrice,
    DateTime EndsAt,
    DateTime OccurredOn
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
