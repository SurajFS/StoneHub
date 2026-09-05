using Catalog.Domain;
using SharedKernel;

namespace Catalog.Application.Products;

// Shared category/subcategory checks for the create + update paths: the primary category must
// exist and be top-level, and a subcategory (if given) must belong to it.
internal static class ProductCategoryValidation
{
    public static async Task<Result> EnsureValidAsync(
        ICategoryRepository categories,
        Guid categoryId,
        Guid? subcategoryId,
        CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Result.NotFound("Category not found.");
        if (category.ParentId is not null)
            return Result.Failure("A subcategory can't be used as the primary category.");

        if (subcategoryId is Guid subId)
        {
            var subcategory = await categories.GetByIdAsync(subId, ct);
            if (subcategory is null)
                return Result.NotFound("Subcategory not found.");
            if (subcategory.ParentId != categoryId)
                return Result.Failure("Subcategory does not belong to the selected category.");
        }

        return Result.Success();
    }
}
