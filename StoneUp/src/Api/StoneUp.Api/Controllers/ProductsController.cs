using Catalog.Application.Dtos;
using Catalog.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneUp.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search(
        [FromQuery] string? materialType,
        [FromQuery] string? keyword,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new ProductSearchFilter(materialType, keyword, minPrice, maxPrice, page, pageSize);
        var results = await mediator.Send(new SearchProductsQuery(filter), ct);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), ct);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> CreateListing(CreateProductListingCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [Authorize(Roles = "Seller")]
    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateStockRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateStockCommand(id, request.QuantityAvailable), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record UpdateStockRequest(decimal QuantityAvailable);
