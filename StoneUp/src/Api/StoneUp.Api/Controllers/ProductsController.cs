using System.Security.Claims;
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
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetMyListings(CancellationToken ct)
    {
        var results = await mediator.Send(new GetMyListingsQuery(CallerSellerId), ct);
        return Ok(results);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> CreateListing(CreateProductListingRequest request, CancellationToken ct)
    {
        var command = new CreateProductListingCommand(
            CallerSellerId,
            request.Title,
            request.MaterialType,
            request.Size,
            request.Thickness,
            request.Finish,
            request.QuantityAvailable,
            request.Price,
            request.Currency,
            request.PhotoUrls,
            request.VideoUrls);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateListing(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            CallerSellerId,
            request.Title,
            request.MaterialType,
            request.Size,
            request.Thickness,
            request.Finish,
            request.Price,
            request.Currency,
            request.QuantityAvailable);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [Authorize(Roles = "Seller")]
    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateStockRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateStockCommand(id, CallerSellerId, request.QuantityAvailable), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    private Guid CallerSellerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record CreateProductListingRequest(
    string Title,
    string MaterialType,
    string Size,
    string Thickness,
    string Finish,
    decimal QuantityAvailable,
    decimal Price,
    string Currency,
    List<string> PhotoUrls,
    List<string> VideoUrls);

public sealed record UpdateProductRequest(
    string Title,
    string MaterialType,
    string Size,
    string Thickness,
    string Finish,
    decimal Price,
    string Currency,
    decimal QuantityAvailable);

public sealed record UpdateStockRequest(decimal QuantityAvailable);
