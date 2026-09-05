using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed record SetProductActiveCommand(Guid ProductId, Guid CallerSellerId, bool IsActive) : IRequest<Result>;
