using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed class GetMyListingsQueryHandler(IProductQueryService productQueryService)
    : IRequestHandler<GetMyListingsQuery, IReadOnlyList<ProductDto>>
{
    public Task<IReadOnlyList<ProductDto>> Handle(GetMyListingsQuery request, CancellationToken ct) =>
        productQueryService.GetBySellerAsync(request.SellerId, ct);
}
