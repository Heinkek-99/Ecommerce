namespace Ecommerce.Shared.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
