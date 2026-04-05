using Ecommerce.Orders.Domain;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Application.Commands.FailOrderPayment;

public class FailOrderPaymentHandler
{
    private readonly OrdersDbContext _db;

    public FailOrderPaymentHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(
        FailOrderPaymentCommand cmd,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);

        if (order is null)
            return Result.Failure($"Order {cmd.OrderId} not found.");

        // Idempotent
        if (order.Status == "payment_failed")
            return Result.Success();

        order.MarkPaymentFailed();

        // Record the failed payment attempt
        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == cmd.OrderId, ct);

        if (existingPayment is null)
        {
            var payment = Payment.Create(cmd.OrderId, "card", order.TotalAmount, order.CurrencyCode);
            payment.Fail();
            _db.Payments.Add(payment);
        }
        else if (existingPayment.Status == "pending")
        {
            existingPayment.Fail();
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();

        // Note: stock reservation/release requires a stock reservation system
        // (PlaceOrderHandler currently does not decrement stock on order creation)
    }
}
