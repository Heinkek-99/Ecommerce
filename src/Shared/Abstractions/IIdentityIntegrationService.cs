using Ecommerce.Shared.Common;

namespace Ecommerce.Shared.Abstractions;

/// <summary>
/// Contrat synchrone pour manipuler les données Identity depuis d'autres modules.
/// Implémenté dans Ecommerce.Identity, consommé par Catalog (RegisterSeller).
/// </summary>
public interface IIdentityIntegrationService : IModuleIntegrationService
{
    Task<Result> AddRoleAsync(Guid userId, string role, CancellationToken ct = default);
}
