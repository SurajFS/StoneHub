using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record UpdateStockCommand(Guid ProductId, decimal QuantityAvailable) : IRequest<Result>;
