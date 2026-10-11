using SharedKernel;

namespace Identity.Domain;

public sealed class WholesalerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public Address? Address { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string? WhatsAppNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private WholesalerProfile() { }

    private WholesalerProfile(
        Guid id,
        Guid userId,
        string businessName,
        Location location,
        Address? address,
        string phone,
        string? whatsAppNumber) : base(id)
    {
        UserId = userId;
        BusinessName = businessName;
        Location = location;
        Address = address;
        Phone = phone;
        WhatsAppNumber = whatsAppNumber;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // `address` is null only for legacy clients that still send free-text city/state.
    public static WholesalerProfile Register(
        Guid userId,
        string businessName,
        Location location,
        Address? address,
        string phone,
        string? whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var normalizedWhatsApp = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        var profile = new WholesalerProfile(
            Guid.NewGuid(), userId, businessName.Trim(), location, address, phone.Trim(), normalizedWhatsApp);
        profile.Raise(new WholesalerRegisteredEvent(profile.Id, userId, profile.BusinessName));
        return profile;
    }

    public void UpdateProfile(
        string businessName, Location location, Address? address, string phone, string? whatsAppNumber, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var previousName = BusinessName;
        var previousLocation = Location;

        BusinessName = businessName.Trim();
        Location = location;
        Address = address;
        Phone = phone.Trim();
        WhatsAppNumber = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();

        RaiseIfDisplayChanged(previousName, previousLocation);
    }

    // Backfill: attach a structured address matched from the existing free-text location.
    public void AttachAddress(Location location, Address address)
    {
        var previousLocation = Location;
        Location = location;
        Address = address;
        RaiseIfDisplayChanged(BusinessName, previousLocation);
    }

    private void RaiseIfDisplayChanged(string previousName, Location previousLocation)
    {
        if (previousName != BusinessName || !previousLocation.Equals(Location))
            Raise(new OwnerProfileChangedEvent(UserId, BusinessName, Location.DisplayName));
    }
}
