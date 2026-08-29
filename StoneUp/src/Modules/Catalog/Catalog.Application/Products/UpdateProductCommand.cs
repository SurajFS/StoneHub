using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record UpdateProductCommand(
    Guid ProductId,
    Guid CallerSellerId,
    string Title,
    string MaterialType,
    string Size,
    string Thickness,
    string Finish,
    decimal Price,
    string Currency,
    decimal QuantityAvailable) : IRequest<Result>;
