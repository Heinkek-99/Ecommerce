namespace Ecommerce.Shared.Abstractions;

/// <summary>
/// Marker interface for synchronous cross-module integration contracts.
/// Implementations live in the target module; consumers reference this interface only.
/// </summary>
public interface IModuleIntegrationService
{
}
