using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categories)
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        // One-level hierarchy: a subcategory's parent must exist and itself be top-level.
        if (request.ParentId is Guid parentId)
        {
            var parent = await categories.GetByIdAsync(parentId, ct);
            if (parent is null)
                return Result.NotFound<Guid>("Parent category not found.");
            if (parent.ParentId is not null)
                return Result.Failure<Guid>("Categories can only nest one level deep.");
        }

        var result = Category.Create(request.Name, request.ParentId, request.DisplayOrder);
        if (result.IsFailure)
            return Result.Failure<Guid>(result);

        var category = result.Value;
        if (await categories.SlugExistsAsync(category.Slug, null, ct))
            return Result.Conflict<Guid>($"A category with slug '{category.Slug}' already exists.");

        categories.Add(category);
        await categories.SaveChangesAsync(ct);
        return Result.Success(category.Id);
    }
}
