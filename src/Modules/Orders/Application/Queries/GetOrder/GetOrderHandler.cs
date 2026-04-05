using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Application.Queries.GetOrder;

public class GetOrderHandler
{
    private readonly OrdersDbContext _db;

    public GetOrderHandler(OrdersDbContext db)
    {
        _db = db;
    }

    public async Task<Result<OrderDto>> Handle(
        GetOrderQuery query,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId && o.UserId == query.UserId, ct);

        if (order is null)
            return Result.Failure<OrderDto>($"Order {query.OrderId} not found.");

        var dto = new OrderDto(
            order.Id,
            order.UserId,
            order.Status,
            order.TotalAmount,
            order.CurrencyCode,
            order.OrderedAt,
            order.Items.Select(i => new OrderItemDto(
                i.VariantId,
                i.ProductName,
                i.ProductSku,
                i.Quantity,
                i.UnitPrice)).ToList());

        return Result.Success(dto);
    }
}
