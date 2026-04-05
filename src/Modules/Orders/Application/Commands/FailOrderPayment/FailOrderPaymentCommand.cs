using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.FailOrderPayment;

public record FailOrderPaymentCommand(
    Guid OrderId,
    string PaymentIntentId
) : ICommand<Result>;
