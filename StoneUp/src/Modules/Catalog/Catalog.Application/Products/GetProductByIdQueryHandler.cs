using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed class GetProductByIdQueryHandler(IProductQueryService productQueryService)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken ct) =>
        productQueryService.GetByIdAsync(request.Id, ct);
}
