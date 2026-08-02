using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed record SearchProductsQuery(ProductSearchFilter Filter) : IRequest<IReadOnlyList<ProductDto>>;
