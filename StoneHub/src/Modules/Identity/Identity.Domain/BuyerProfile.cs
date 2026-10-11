using SharedKernel;

namespace Identity.Domain;

// Buyers carry a display City and, since structured addresses, an optional State + Address.
// Legacy buyer profiles (city only) have neither until backfilled or edited.
public sealed class BuyerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public BuyerType BuyerType { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? State { get; private set; }
    public Address? Address { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private BuyerProfile() { }

    private BuyerProfile(
        Guid id, Guid userId, string displayName, BuyerType buyerType, string city, string? state, Address? address)
        : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        BuyerType = buyerType;
        City = city;
        State = state;
        Address = address;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static BuyerProfile Register(
        Guid userId, string displayName, BuyerType buyerType, string city, string? state, Address? address)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        return new BuyerProfile(
            Guid.NewGuid(), userId, displayName.Trim(), buyerType, city.Trim(), TrimToNull(state), address);
    }

    public void UpdateProfile(string displayName, string city, string? state, Address? address, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        DisplayName = displayName.Trim();
        City = city.Trim();
        State = TrimToNull(state);
        Address = address;
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
    }

    // Backfill: attach a structured address matched from the existing free-text city.
    public void AttachAddress(string city, string state, Address address)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        City = city.Trim();
        State = TrimToNull(state);
        Address = address;
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
