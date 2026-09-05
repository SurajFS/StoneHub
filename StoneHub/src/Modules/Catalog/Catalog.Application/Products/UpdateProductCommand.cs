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
    decimal QuantityAvailable) : IRequest<Result>;
