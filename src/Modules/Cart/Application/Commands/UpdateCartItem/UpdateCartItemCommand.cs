using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.UpdateCartItem;

public record UpdateCartItemCommand(Guid UserId, Guid VariantId, int NewQuantity) : ICommand<Result>;
