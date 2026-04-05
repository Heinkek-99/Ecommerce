using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.RemoveFromCart;

public record RemoveFromCartCommand(Guid UserId, Guid VariantId) : ICommand<Result>;
