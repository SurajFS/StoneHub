using SharedKernel;

namespace Identity.Domain;

public sealed class BuyerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public BuyerType BuyerType { get; private set; }
    public string City { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private BuyerProfile() { }

    private BuyerProfile(Guid id, Guid userId, string displayName, BuyerType buyerType, string city) : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        BuyerType = buyerType;
        City = city;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static BuyerProfile Register(Guid userId, string displayName, BuyerType buyerType, string city)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        return new BuyerProfile(Guid.NewGuid(), userId, displayName.Trim(), buyerType, city.Trim());
    }
}
