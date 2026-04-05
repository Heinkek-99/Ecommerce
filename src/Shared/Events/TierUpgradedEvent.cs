namespace Ecommerce.Shared.Events;

public record TierUpgradedEvent(
    Guid UserId,
    string OldTier,
    string NewTier,
    DateTime OccurredOn
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
