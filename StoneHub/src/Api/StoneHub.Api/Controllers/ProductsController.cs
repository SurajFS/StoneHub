using System.Security.Claims;
using Catalog.Application.Dtos;
using Catalog.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace StoneHub.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    // Browse is role-scoped: the visible owner type is derived from the caller's role and forced
    // onto the query. Buyers and wholesalers see seller listings; sellers see wholesaler listings.
    // Any client-supplied owner type is ignored — it is an authorization decision, not a filter.
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search(
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? subcategoryId,
        [FromQuery] string? keyword,
        [FromQuery] string? sellerName,
        [FromQuery] string? location,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool inStockOnly = false,
        [FromQuery] decimal? maxMoq = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new ProductSearchFilter(
            categoryId, subcategoryId, keyword, sellerName, location, VisibleOwnerType,
            minPrice, maxPrice, inStockOnly, maxMoq, sortBy, page, pageSize);
        var results = await mediator.Send(new SearchProductsQuery(filter), ct);
        return Ok(results);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (product is null)
            return NotFound();

        // Owners always see their own listing; otherwise a caller may only open a listing whose
        // owner type is visible to their role. Return 404 (not 403) so a hidden listing's
        // existence isn't revealed.
        var isOwner = product.SellerId == CallerSellerId;
        if (!isOwner && !string.Equals(product.OwnerType, VisibleOwnerType, StringComparison.OrdinalIgnoreCase))
            return NotFound();

        return Ok(product);
    }

    [Authorize(Roles = "Seller,Wholesaler")]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetMyListings(CancellationToken ct)
    {
        var results = await mediator.Send(new GetMyListingsQuery(CallerSellerId), ct);
        return Ok(results);
    }

    [Authorize(Roles = "Seller,Wholesaler")]
    [HttpPost]
    public async Task<IActionResult> CreateListing(CreateProductListingRequest request, CancellationToken ct)
    {
        var command = new CreateProductListingCommand(
            CallerSellerId,
            OwnerType: CallerOwnerType,
            request.Title,
            request.CategoryId,
            request.SubcategoryId,
            request.Size,
            request.Thickness,
            request.Finish,
            request.Color,
            request.Unit,
            request.Tags ?? [],
            request.QuantityAvailable,
            request.Price,
            request.Currency,
            request.WholesalePrice,
            request.MinimumOrderQuantity,
            request.PhotoUrls ?? [],
            request.VideoUrls ?? []);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : result.ToActionResult();
    }

    [Authorize(Roles = "Seller,Wholesaler")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateListing(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            CallerSellerId,
            request.Title,
            request.CategoryId,
            request.SubcategoryId,
            request.Size,
            request.Thickness,
            request.Finish,
            request.Color,
            request.Unit,
            request.Tags ?? [],
            request.Price,
            request.Currency,
            request.WholesalePrice,
            request.MinimumOrderQuantity,
            request.QuantityAvailable);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    [Authorize(Roles = "Seller,Wholesaler")]
    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateStockRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateStockCommand(id, CallerSellerId, request.QuantityAvailable), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    [Authorize(Roles = "Seller,Wholesaler")]
    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, SetProductActiveRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SetProductActiveCommand(id, CallerSellerId, request.IsActive), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    private Guid CallerSellerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // A listing's owner type follows the caller's role; wholesalers list wholesale supply.
    private string CallerOwnerType => User.IsInRole("Wholesaler") ? "Wholesaler" : "Seller";

    // The single listing owner type a caller may browse/open, derived from their role:
    // sellers source stock from wholesalers, while buyers and wholesalers browse seller listings.
    private string VisibleOwnerType => User.IsInRole("Seller") ? "Wholesaler" : "Seller";
}

public sealed record CreateProductListingRequest(
    string Title,
    Guid CategoryId,
    Guid? SubcategoryId,
    string? Size,
    string? Thickness,
    string? Finish,
    string? Color,
    string Unit,
    List<string>? Tags,
    decimal QuantityAvailable,
    decimal Price,
    string Currency,
    decimal? WholesalePrice,
    decimal? MinimumOrderQuantity,
    List<string>? PhotoUrls,
    List<string>? VideoUrls);

public sealed record UpdateProductRequest(
    string Title,
    Guid CategoryId,
    Guid? SubcategoryId,
    string? Size,
    string? Thickness,
    string? Finish,
    string? Color,
    string Unit,
    List<string>? Tags,
    decimal Price,
    string Currency,
    decimal? WholesalePrice,
    decimal? MinimumOrderQuantity,
    decimal QuantityAvailable);

public sealed record UpdateStockRequest(decimal QuantityAvailable);

public sealed record SetProductActiveRequest(bool IsActive);
