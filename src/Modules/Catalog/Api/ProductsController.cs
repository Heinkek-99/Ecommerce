using Ecommerce.Catalog.Application.Commands.CreateProduct;
using Ecommerce.Catalog.Application.Queries.GetProduct;
<<<<<<< HEAD
using Ecommerce.Catalog.Application.Queries.GetProductBySlug;
using Ecommerce.Catalog.Application.Queries.ListProducts;
=======
>>>>>>> feature/orders-logic
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Catalog.Api;

[ApiController]
[Route("api/catalog/products")]
<<<<<<< HEAD
=======
[Authorize]
>>>>>>> feature/orders-logic
public class ProductsController : ControllerBase
{
    private readonly IMessageBus _bus;

<<<<<<< HEAD
    public ProductsController(IMessageBus bus) => _bus = bus;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? categorySlug,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] Guid? sellerId,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ListProductsQuery(search, categorySlug, minPrice, maxPrice, sellerId, sortBy, page, pageSize);
        var result = await _bus.InvokeAsync<Result<PagedList<ProductListDto>>>(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _bus.InvokeAsync<Result<ProductDetailDto>>(new GetProductBySlugQuery(slug));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("by-id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _bus.InvokeAsync<Result<ProductDto>>(new GetProductQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    [Authorize]
=======
    public ProductsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
>>>>>>> feature/orders-logic
    public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd)
    {
        var result = await _bus.InvokeAsync<Result<Guid>>(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
<<<<<<< HEAD
=======

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _bus.InvokeAsync<Result<ProductDto>>(new GetProductQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
>>>>>>> feature/orders-logic
}
