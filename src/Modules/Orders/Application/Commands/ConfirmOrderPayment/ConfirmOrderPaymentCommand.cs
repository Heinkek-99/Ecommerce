using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.ConfirmOrderPayment;

public record ConfirmOrderPaymentCommand(
    Guid OrderId,
    string PaymentIntentId
) : ICommand<Result>;
