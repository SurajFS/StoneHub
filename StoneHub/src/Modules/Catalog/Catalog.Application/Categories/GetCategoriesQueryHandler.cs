using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Categories;

public sealed class GetCategoriesQueryHandler(ICategoryQueryService categoryQueryService)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct) =>
        categoryQueryService.GetAllAsync(request.IncludeInactive, ct);
}
