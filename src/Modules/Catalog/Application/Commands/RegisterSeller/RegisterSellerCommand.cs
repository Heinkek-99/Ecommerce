using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.RegisterSeller;

public record RegisterSellerCommand(
    Guid UserId,
    string BusinessName,
    string Email,
    string? Phone = null,
    string? Address = null
) : ICommand<Result<Guid>>;
