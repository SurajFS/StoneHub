using SharedKernel;

namespace Catalog.Domain;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductMedia> _media = [];

    public Guid SellerId { get; private set; }
    public ListingOwnerType OwnerType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Guid? SubcategoryId { get; private set; }
    public string? Size { get; private set; }
    public string? Thickness { get; private set; }
    public string? Finish { get; private set; }
    public string? Color { get; private set; }
    public ProductUnit Unit { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public decimal QuantityAvailable { get; private set; }
    public Money Price { get; private set; } = null!;
    // Wholesale amount uses the same currency as Price; null when the listing isn't offered wholesale.
    public decimal? WholesalePrice { get; private set; }
    public decimal? MinimumOrderQuantity { get; private set; }
    // Denormalized owner display info, captured at listing time (kept in sync by profile events later).
    public string? SellerName { get; private set; }
    public string? SellerLocation { get; private set; }
    public bool IsAvailable { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<ProductMedia> Media => _media.AsReadOnly();

    private Product() { }

    private Product(
        Guid id,
        Guid sellerId,
        ListingOwnerType ownerType,
        string title,
        Guid categoryId,
        Guid? subcategoryId,
        string? size,
        string? thickness,
        string? finish,
        string? color,
        ProductUnit unit,
        IEnumerable<string> tags,
        decimal quantityAvailable,
        Money price,
        decimal? wholesalePrice,
        decimal? minimumOrderQuantity) : base(id)
    {
        SellerId = sellerId;
        OwnerType = ownerType;
        Title = title;
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        Size = size;
        Thickness = thickness;
        Finish = finish;
        Color = color;
        Unit = unit;
        Tags = NormalizeTags(tags);
        QuantityAvailable = quantityAvailable;
        Price = price;
        WholesalePrice = wholesalePrice;
        MinimumOrderQuantity = minimumOrderQuantity;
        IsAvailable = quantityAvailable > 0;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Result<Product> CreateListing(
        Guid sellerId,
        ListingOwnerType ownerType,
        string title,
        Guid categoryId,
        Guid? subcategoryId,
        string? size,
        string? thickness,
        string? finish,
        string? color,
        ProductUnit unit,
        IEnumerable<string> tags,
        decimal quantityAvailable,
        Money price,
        decimal? wholesalePrice,
        decimal? minimumOrderQuantity)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;
        if (trimmedTitle.Length == 0)
            return Result.Failure<Product>("Title is required.");
        if (categoryId == Guid.Empty)
            return Result.Failure<Product>("Category is required.");
        if (quantityAvailable < 0)
            return Result.Failure<Product>("Quantity cannot be negative.");

        var wholesale = ValidateWholesale(wholesalePrice, minimumOrderQuantity);
        if (wholesale.IsFailure)
            return Result.Failure<Product>(wholesale);

        var product = new Product(
            Guid.NewGuid(), sellerId, ownerType, trimmedTitle, categoryId, subcategoryId,
            Normalize(size), Normalize(thickness), Normalize(finish), Normalize(color),
            unit, tags, quantityAvailable, price, wholesalePrice, minimumOrderQuantity);

        product.Raise(new ProductListedEvent(product.Id, sellerId, product.Title));
        return Result.Success(product);
    }

    public Result UpdateDetails(
        string title,
        Guid categoryId,
        Guid? subcategoryId,
        string? size,
        string? thickness,
        string? finish,
        string? color,
        ProductUnit unit,
        IEnumerable<string> tags,
        Money price,
        decimal? wholesalePrice,
        decimal? minimumOrderQuantity)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;
        if (trimmedTitle.Length == 0)
            return Result.Failure("Title is required.");
        if (categoryId == Guid.Empty)
            return Result.Failure("Category is required.");

        var wholesale = ValidateWholesale(wholesalePrice, minimumOrderQuantity);
        if (wholesale.IsFailure)
            return wholesale;

        Title = trimmedTitle;
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        Size = Normalize(size);
        Thickness = Normalize(thickness);
        Finish = Normalize(finish);
        Color = Normalize(color);
        Unit = unit;
        Tags = NormalizeTags(tags);
        Price = price;
        WholesalePrice = wholesalePrice;
        MinimumOrderQuantity = minimumOrderQuantity;
        return Result.Success();
    }

    public void UpdateStock(decimal quantityAvailable)
    {
        if (quantityAvailable < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantityAvailable));

        QuantityAvailable = quantityAvailable;
        IsAvailable = quantityAvailable > 0;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void AddMedia(ProductMedia media) => _media.Add(media);

    // Denormalized so search and product cards can show the owner without crossing into Identity.
    public void SetSellerInfo(string? sellerName, string? sellerLocation)
    {
        SellerName = Normalize(sellerName);
        SellerLocation = Normalize(sellerLocation);
    }

    // Wholesale price and MOQ are all-or-nothing; when present, both must be sensible.
    private static Result ValidateWholesale(decimal? wholesalePrice, decimal? minimumOrderQuantity)
    {
        if (wholesalePrice is null && minimumOrderQuantity is null)
            return Result.Success();
        if (wholesalePrice is null || minimumOrderQuantity is null)
            return Result.Failure("Wholesale price and minimum order quantity must be set together.");
        if (wholesalePrice <= 0)
            return Result.Failure("Wholesale price must be greater than zero.");
        if (minimumOrderQuantity < 1)
            return Result.Failure("Minimum order quantity must be at least 1.");
        return Result.Success();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> NormalizeTags(IEnumerable<string> tags) =>
        tags
            .Select(tag => tag?.Trim().ToLowerInvariant() ?? string.Empty)
            .Where(tag => tag.Length > 0)
            .Distinct()
            .ToList();
}
