using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.ClearCart;

public record ClearCartCommand(Guid UserId) : ICommand<Result>;
