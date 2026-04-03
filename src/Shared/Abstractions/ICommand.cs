namespace Ecommerce.Shared.Abstractions;

/// <summary>
/// Marker interface for commands (CQRS — write side).
/// Wolverine discovers handlers by convention (Handle method).
/// </summary>
public interface ICommand { }

public interface ICommand<TResult> { }

public interface IQuery<TResult> { }
