using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Categories;

public sealed class SetCategoryActiveCommandHandler(ICategoryRepository categories)
    : IRequestHandler<SetCategoryActiveCommand, Result>
{
    public async Task<Result> Handle(SetCategoryActiveCommand request, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result.NotFound("Category not found.");

        if (request.IsActive)
            category.Activate();
        else
            category.Deactivate();

        await categories.SaveChangesAsync(ct);
        return Result.Success();
    }
}
