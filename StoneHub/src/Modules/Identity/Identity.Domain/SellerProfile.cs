using SharedKernel;

namespace Identity.Domain;

public sealed class SellerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public Address? Address { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string? WhatsAppNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private SellerProfile() { }

    private SellerProfile(
        Guid id,
        Guid userId,
        string companyName,
        Location location,
        Address? address,
        string phone,
        string? whatsAppNumber) : base(id)
    {
        UserId = userId;
        CompanyName = companyName;
        Location = location;
        Address = address;
        Phone = phone;
        WhatsAppNumber = whatsAppNumber;
        IsVerified = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // `address` is null only for legacy clients that still send free-text city/state.
    public static SellerProfile Register(
        Guid userId,
        string companyName,
        Location location,
        Address? address,
        string phone,
        string? whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var normalizedWhatsApp = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        var profile = new SellerProfile(
            Guid.NewGuid(), userId, companyName.Trim(), location, address, phone.Trim(), normalizedWhatsApp);
        profile.Raise(new SellerRegisteredEvent(profile.Id, userId, profile.CompanyName));
        return profile;
    }

    public void MarkVerified() => IsVerified = true;

    public void UpdateProfile(
        string companyName, Location location, Address? address, string phone, string? whatsAppNumber, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var previousName = CompanyName;
        var previousLocation = Location;

        CompanyName = companyName.Trim();
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
        RaiseIfDisplayChanged(CompanyName, previousLocation);
    }

    private void RaiseIfDisplayChanged(string previousName, Location previousLocation)
    {
        if (previousName != CompanyName || !previousLocation.Equals(Location))
            Raise(new OwnerProfileChangedEvent(UserId, CompanyName, Location.DisplayName));
    }
}
