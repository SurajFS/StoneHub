using Catalog.Application.Dtos;
using MediatR;

namespace Catalog.Application.Categories;

public sealed record GetCategoriesQuery(bool IncludeInactive) : IRequest<IReadOnlyList<CategoryDto>>;
