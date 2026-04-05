namespace Ecommerce.Shared.Events;

public record OrderConfirmedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string CurrencyCode,
    string? PromoCode,
    DateTime OccurredOn
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
