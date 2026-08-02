using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record CreateProductListingCommand(
    Guid SellerId,
    string Title,
    string MaterialType,
    string Size,
    string Thickness,
    string Finish,
    decimal QuantityAvailable,
    decimal Price,
    string Currency,
    List<string> PhotoUrls,
    List<string> VideoUrls) : IRequest<Result<Guid>>;
