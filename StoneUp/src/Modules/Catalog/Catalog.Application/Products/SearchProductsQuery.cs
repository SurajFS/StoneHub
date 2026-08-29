using Catalog.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record SearchProductsQuery(ProductSearchFilter Filter) : IRequest<PagedResult<ProductDto>>;
