using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed class SearchProductsQueryHandler(IProductQueryService productQueryService)
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    public Task<IReadOnlyList<ProductDto>> Handle(SearchProductsQuery request, CancellationToken ct) =>
        productQueryService.SearchAsync(request.Filter, ct);
}
