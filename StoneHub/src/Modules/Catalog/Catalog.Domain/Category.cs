using System.Text;
using SharedKernel;

namespace Catalog.Domain;

// A product category (e.g. Marble, Handicrafts) or one-level subcategory (ParentId set).
// Admin-managed reference data; products reference a category by Id, never by name/slug.
public sealed class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Category() { }

    private Category(Guid id, string name, string slug, Guid? parentId, int displayOrder) : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public static Result<Category> Create(string name, Guid? parentId, int displayOrder)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return Result.Failure<Category>("Category name is required.");

        var slug = Slugify(trimmed);
        if (slug.Length == 0)
            return Result.Failure<Category>("Category name must contain at least one letter or digit.");

        return Result.Success(new Category(Guid.NewGuid(), trimmed, slug, parentId, Math.Max(0, displayOrder)));
    }

    public Result Rename(string name, int displayOrder)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return Result.Failure("Category name is required.");

        var slug = Slugify(trimmed);
        if (slug.Length == 0)
            return Result.Failure("Category name must contain at least one letter or digit.");

        Name = trimmed;
        Slug = slug;
        DisplayOrder = Math.Max(0, displayOrder);
        return Result.Success();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    // URL-safe slug: lowercase, alphanumeric runs joined by single hyphens ("Kota Stone" → "kota-stone").
    public static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastWasHyphen = false;

        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
