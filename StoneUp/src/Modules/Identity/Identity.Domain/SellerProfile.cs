using SharedKernel;

namespace Identity.Domain;

public sealed class SellerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;
    public string? WhatsAppNumber { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private SellerProfile() { }

    private SellerProfile(
        Guid id,
        Guid userId,
        string companyName,
        Location location,
        string phone,
        string? whatsAppNumber) : base(id)
    {
        UserId = userId;
        CompanyName = companyName;
        Location = location;
        Phone = phone;
        WhatsAppNumber = whatsAppNumber;
        IsVerified = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static SellerProfile Register(
        Guid userId,
        string companyName,
        Location location,
        string phone,
        string? whatsAppNumber)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));

        var normalizedWhatsApp = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : whatsAppNumber.Trim();
        var profile = new SellerProfile(
            Guid.NewGuid(), userId, companyName.Trim(), location, phone.Trim(), normalizedWhatsApp);
        profile.Raise(new SellerRegisteredEvent(profile.Id, userId, profile.CompanyName));
        return profile;
    }

    public void MarkVerified() => IsVerified = true;

    public void UpdateProfile(string companyName, Location location)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));

        CompanyName = companyName.Trim();
        Location = location;
    }
}
