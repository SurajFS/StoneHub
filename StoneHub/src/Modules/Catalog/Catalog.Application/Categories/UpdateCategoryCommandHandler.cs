using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categories)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result.NotFound("Category not found.");

        var newSlug = Category.Slugify(request.Name);
        if (await categories.SlugExistsAsync(newSlug, request.Id, ct))
            return Result.Conflict($"A category with slug '{newSlug}' already exists.");

        var renamed = category.Rename(request.Name, request.DisplayOrder);
        if (renamed.IsFailure)
            return renamed;

        await categories.SaveChangesAsync(ct);
        return Result.Success();
    }
}
