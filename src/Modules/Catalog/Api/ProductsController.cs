using Ecommerce.Catalog.Application.Commands.CreateProduct;
using Ecommerce.Catalog.Application.Queries.GetProduct;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Catalog.Api;

[ApiController]
[Route("api/catalog/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public ProductsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd)
    {
        var result = await _bus.InvokeAsync<Result<Guid>>(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _bus.InvokeAsync<Result<ProductDto>>(new GetProductQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
