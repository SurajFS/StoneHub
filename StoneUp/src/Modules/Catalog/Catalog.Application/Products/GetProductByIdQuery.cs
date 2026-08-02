using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Products;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
