using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Orders.Application.Queries.GetSellerOrders;

public class GetSellerOrdersHandler
{
    private readonly OrdersDbContext _db;

    public GetSellerOrdersHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<PagedList<SellerOrderDto>>> Handle(
        GetSellerOrdersQuery query,
        CancellationToken ct)
    {
        var orderIds = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.SellerId == query.SellerId)
            .Select(i => i.OrderId)
            .Distinct()
            .ToListAsync(ct);

        var q = _db.Orders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id));

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(o => o.Status == query.Status);

        q = q.OrderByDescending(o => o.OrderedAt);

        var total = await q.CountAsync(ct);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var orders = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.SellerId == query.SellerId && orderIds.Contains(i.OrderId))
            .ToListAsync(ct);

        var dtos = orders.Select(o => new SellerOrderDto(
            o.Id, o.UserId, o.Status, o.TotalAmount, o.CurrencyCode, o.OrderedAt,
            items.Where(i => i.OrderId == o.Id)
                 .Select(i => new SellerOrderItemDto(i.VariantId, i.ProductName, i.ProductSku, i.Quantity, i.UnitPrice))
                 .ToList()))
            .ToList();

        return Result.Success(new PagedList<SellerOrderDto>(dtos, total, page, pageSize));
    }
}
