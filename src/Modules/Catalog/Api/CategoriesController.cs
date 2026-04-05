using Ecommerce.Catalog.Application.Queries.GetCategoriesTree;
using Ecommerce.Catalog.Application.Queries.GetCategoryProducts;
using Ecommerce.Catalog.Application.Queries.ListProducts;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Catalog.Api;

[ApiController]
[Route("api/catalog/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMessageBus _bus;

    public CategoriesController(IMessageBus bus) => _bus = bus;

    [HttpGet]
    public async Task<IActionResult> GetTree()
    {
        var result = await _bus.InvokeAsync<Result<List<CategoryTreeDto>>>(new GetCategoriesTreeQuery());
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, result.Error);
    }

    [HttpGet("{slug}/products")]
    public async Task<IActionResult> GetCategoryProducts(
        string slug,
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] Guid? sellerId,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetCategoryProductsQuery(slug, search, minPrice, maxPrice, sellerId, sortBy, page, pageSize);
        var result = await _bus.InvokeAsync<Result<PagedList<ProductListDto>>>(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
