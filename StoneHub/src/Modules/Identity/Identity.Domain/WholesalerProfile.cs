using SharedKernel;

namespace Identity.Domain;

public sealed class WholesalerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;
    public string? WhatsAppNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private WholesalerProfile() { }

    private WholesalerProfile(
        Guid id,
        Guid userId,
        string businessName,
        Location location,
        string phone,
        string? whatsAppNumber) : base(id)
    {
        UserId = userId;
        BusinessName = businessName;
        Location = location;
        Phone = phone;
        WhatsAppNumber = whatsAppNumber;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static WholesalerProfile Register(
        Guid userId,
        string businessName,
        Location location,
        string phone,
        string? whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var normalizedWhatsApp = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        var profile = new WholesalerProfile(
            Guid.NewGuid(), userId, businessName.Trim(), location, phone.Trim(), normalizedWhatsApp);
        profile.Raise(new WholesalerRegisteredEvent(profile.Id, userId, profile.BusinessName));
        return profile;
    }

    public void UpdateProfile(string businessName, Location location)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));

        BusinessName = businessName.Trim();
        Location = location;
    }
}
