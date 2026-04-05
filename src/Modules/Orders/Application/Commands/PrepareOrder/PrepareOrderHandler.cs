using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Application.Commands.PrepareOrder;

public class PrepareOrderHandler
{
    private readonly OrdersDbContext _db;

    public PrepareOrderHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(PrepareOrderCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null) return Result.Failure($"Order {cmd.OrderId} not found.");

        var belongsToSeller = await _db.OrderItems
            .AnyAsync(i => i.OrderId == cmd.OrderId && i.SellerId == cmd.SellerId, ct);
        if (!belongsToSeller) return Result.Failure("Order does not belong to this seller.");

        try { order.Prepare(); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
