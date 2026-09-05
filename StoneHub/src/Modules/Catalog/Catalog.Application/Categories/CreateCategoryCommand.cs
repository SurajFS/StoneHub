using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed record CreateCategoryCommand(string Name, Guid? ParentId, int DisplayOrder) : IRequest<Result<Guid>>;
