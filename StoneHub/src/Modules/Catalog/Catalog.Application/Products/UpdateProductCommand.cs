using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record UpdateProductCommand(
    Guid ProductId,
    Guid CallerSellerId,
    string Title,
    Guid CategoryId,
    Guid? SubcategoryId,
    string? Size,
    string? Thickness,
    string? Finish,
    string? Color,
    string Unit,
    List<string> Tags,
    decimal Price,
    string Currency,
    decimal? WholesalePrice,
    decimal? MinimumOrderQuantity,
    decimal QuantityAvailable,
    // Null for both = leave media untouched (older clients); otherwise the listing's media is
    // replaced with exactly these, a null list meaning none of that kind.
    List<string>? PhotoUrls,
    List<string>? VideoUrls) : IRequest<Result>;
