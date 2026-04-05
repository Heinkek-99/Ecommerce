using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone = null,
    string Role = "buyer"
) : ICommand<Result<Guid>>;
