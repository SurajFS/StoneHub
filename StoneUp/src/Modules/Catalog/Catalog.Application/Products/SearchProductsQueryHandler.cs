using Catalog.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class SearchProductsQueryHandler(IProductQueryService productQueryService)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    public Task<PagedResult<ProductDto>> Handle(SearchProductsQuery request, CancellationToken ct) =>
        productQueryService.SearchAsync(request.Filter, ct);
}
