using SharedKernel;

namespace Catalog.Domain;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductMedia> _media = [];

    public Guid SellerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public MaterialType MaterialType { get; private set; }
    public string Size { get; private set; } = string.Empty;
    public string Thickness { get; private set; } = string.Empty;
    public string Finish { get; private set; } = string.Empty;
    public decimal QuantityAvailable { get; private set; }
    public Money Price { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<ProductMedia> Media => _media.AsReadOnly();

    private Product() { }

    private Product(
        Guid id,
        Guid sellerId,
        string title,
        MaterialType materialType,
        string size,
        string thickness,
        string finish,
        decimal quantityAvailable,
        Money price) : base(id)
    {
        SellerId = sellerId;
        Title = title;
        MaterialType = materialType;
        Size = size;
        Thickness = thickness;
        Finish = finish;
        QuantityAvailable = quantityAvailable;
        Price = price;
        IsAvailable = quantityAvailable > 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Product CreateListing(
        Guid sellerId,
        string title,
        MaterialType materialType,
        string size,
        string thickness,
        string finish,
        decimal quantityAvailable,
        Money price)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (quantityAvailable < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantityAvailable));

        var product = new Product(
            Guid.NewGuid(), sellerId, title.Trim(), materialType, size, thickness, finish, quantityAvailable, price);

        product.Raise(new ProductListedEvent(product.Id, sellerId, product.Title));
        return product;
    }

    public void AddMedia(ProductMedia media) => _media.Add(media);

    public void UpdateDetails(
        string title,
        MaterialType materialType,
        string size,
        string thickness,
        string finish,
        Money price)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        MaterialType = materialType;
        Size = size;
        Thickness = thickness;
        Finish = finish;
        Price = price;
    }

    public void UpdateStock(decimal quantityAvailable)
    {
        if (quantityAvailable < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantityAvailable));

        QuantityAvailable = quantityAvailable;
        IsAvailable = quantityAvailable > 0;
    }
}
