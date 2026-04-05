using Ecommerce.Orders.Domain;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Orders.Application.Commands.ConfirmOrderPayment;

public class ConfirmOrderPaymentHandler
{
    private readonly OrdersDbContext _db;

    public ConfirmOrderPaymentHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(
        ConfirmOrderPaymentCommand cmd,
        IMessageBus bus,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);

        if (order is null)
            return Result.Failure($"Order {cmd.OrderId} not found.");

        // Idempotent — already confirmed (e.g. by PlaceOrderHandler in sync flow)
        if (order.Status == "confirmed")
            return Result.Success();

        if (order.Status != "pending")
            return Result.Failure($"Cannot confirm order in status '{order.Status}'.");

        order.Confirm();

        // Upsert payment record
        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == cmd.OrderId, ct);

        if (existingPayment is null)
        {
            var payment = Payment.Create(cmd.OrderId, "card", order.TotalAmount, order.CurrencyCode);
            payment.Complete(cmd.PaymentIntentId);
            _db.Payments.Add(payment);
        }
        else if (existingPayment.Status != "completed")
        {
            existingPayment.Complete(cmd.PaymentIntentId);
        }

        // OUTBOX : event avant SaveChanges
        await bus.PublishAsync(new OrderConfirmedEvent(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.CurrencyCode,
            PromoCode: null,
            DateTime.UtcNow));

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
