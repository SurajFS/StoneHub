using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed record GetMyListingsQuery(Guid SellerId) : IRequest<IReadOnlyList<ProductDto>>;
