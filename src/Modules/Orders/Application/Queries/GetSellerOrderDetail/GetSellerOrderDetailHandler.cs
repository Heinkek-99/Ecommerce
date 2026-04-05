using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Application.Queries.GetSellerOrderDetail;

public class GetSellerOrderDetailHandler
{
    private readonly OrdersDbContext _db;

    public GetSellerOrderDetailHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<SellerOrderDetailDto>> Handle(
        GetSellerOrderDetailQuery query,
        CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, ct);

        if (order is null)
            return Result.Failure<SellerOrderDetailDto>($"Order {query.OrderId} not found.");

        var items = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.OrderId == query.OrderId && i.SellerId == query.SellerId)
            .ToListAsync(ct);

        if (items.Count == 0)
            return Result.Failure<SellerOrderDetailDto>("No items belonging to this seller in the order.");

        var dto = new SellerOrderDetailDto(
            order.Id, order.UserId, order.Status,
            order.TotalAmount, order.CurrencyCode,
            order.AddressId, order.OrderedAt, order.UpdatedAt,
            items.Select(i => new SellerOrderDetailItemDto(
                i.VariantId, i.ProductName, i.ProductSku,
                i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList());

        return Result.Success(dto);
    }
}
