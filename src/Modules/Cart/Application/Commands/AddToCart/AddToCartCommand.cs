using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.AddToCart;

public record AddToCartCommand(Guid UserId, Guid VariantId, int Quantity) : ICommand<Result>;
