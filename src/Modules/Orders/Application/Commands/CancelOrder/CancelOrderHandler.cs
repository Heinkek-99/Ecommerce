using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Orders.Application.Commands.CancelOrder;

public class CancelOrderHandler
{
    private readonly OrdersDbContext _db;

    public CancelOrderHandler(OrdersDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(
        CancelOrderCommand cmd,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId && o.UserId == cmd.UserId, ct);

        if (order is null)
            return Result.Failure($"Order {cmd.OrderId} not found.");

        order.Cancel();
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
