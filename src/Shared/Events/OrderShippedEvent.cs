namespace Ecommerce.Shared.Events;

public record OrderShippedEvent(
    Guid OrderId,
    Guid UserId,
    string TrackingNumber,
    DateTime OccurredOn
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
