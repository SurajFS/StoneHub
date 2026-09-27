using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record DeleteProductCommand(Guid ProductId, Guid CallerSellerId) : IRequest<Result>;
