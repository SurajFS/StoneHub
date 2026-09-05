using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed record UpdateCategoryCommand(Guid Id, string Name, int DisplayOrder) : IRequest<Result>;
