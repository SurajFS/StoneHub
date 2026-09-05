using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record CreateProductListingCommand(
    Guid SellerId,
    string OwnerType,
    string Title,
    Guid CategoryId,
    Guid? SubcategoryId,
    string? Size,
    string? Thickness,
    string? Finish,
    string? Color,
    string Unit,
    List<string> Tags,
    decimal QuantityAvailable,
    decimal Price,
    string Currency,
    decimal? WholesalePrice,
    decimal? MinimumOrderQuantity,
    List<string> PhotoUrls,
    List<string> VideoUrls) : IRequest<Result<Guid>>;
