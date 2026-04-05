using Ecommerce.Orders.Domain;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Events;
using Wolverine;

namespace Ecommerce.Orders.Application.Commands.PlaceOrder;

public class PlaceOrderHandler
{
    private readonly OrdersDbContext _db;

    public PlaceOrderHandler(OrdersDbContext db)
    {
        _db = db;
    }

    public async Task<Result<Guid>> Handle(
        PlaceOrderCommand cmd,
        IMessageBus bus,
        CancellationToken ct)
    {
        if (cmd.Lines is null || cmd.Lines.Count == 0)
            return Result.Failure<Guid>("Order must have at least one item.");

        var order = Order.Create(
            cmd.UserId,
            cmd.AddressId,
            cmd.CurrencyCode,
            cmd.Lines);

        order.Confirm();
        _db.Orders.Add(order);

        // OUTBOX : bus.PublishAsync() TOUJOURS AVANT SaveChangesAsync()
        // Wolverine enregistre l'event dans wolverine.outbox_messages
        // dans la MÊME transaction que l'order → atomique
        await bus.PublishAsync(new OrderConfirmedEvent(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.CurrencyCode,
            cmd.PromoCode,
            DateTime.UtcNow));

        await _db.SaveChangesAsync(ct);

        return Result.Success(order.Id);
    }
}
