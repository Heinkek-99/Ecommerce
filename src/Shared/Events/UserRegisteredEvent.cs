namespace Ecommerce.Shared.Events;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime OccurredOn
) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
