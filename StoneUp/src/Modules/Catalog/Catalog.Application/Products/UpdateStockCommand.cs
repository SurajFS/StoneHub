using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record UpdateStockCommand(Guid ProductId, Guid CallerSellerId, decimal QuantityAvailable) : IRequest<Result>;
