using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed record SetCategoryActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
