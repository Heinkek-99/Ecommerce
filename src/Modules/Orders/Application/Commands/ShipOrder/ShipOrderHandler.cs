using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Orders.Application.Commands.ShipOrder;

public class ShipOrderHandler
{
    private readonly OrdersDbContext _db;

    public ShipOrderHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(ShipOrderCommand cmd, IMessageBus bus, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null) return Result.Failure($"Order {cmd.OrderId} not found.");

        var belongsToSeller = await _db.OrderItems
            .AnyAsync(i => i.OrderId == cmd.OrderId && i.SellerId == cmd.SellerId, ct);
        if (!belongsToSeller) return Result.Failure("Order does not belong to this seller.");

        try { order.Ship(); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        // OUTBOX : event AVANT SaveChanges
        await bus.PublishAsync(new OrderShippedEvent(
            order.Id,
            order.UserId,
            cmd.TrackingNumber,
            DateTime.UtcNow));

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
