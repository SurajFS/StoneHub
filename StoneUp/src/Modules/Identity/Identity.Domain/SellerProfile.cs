using SharedKernel;

namespace Identity.Domain;

public sealed class SellerProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public bool IsVerified { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private SellerProfile() { }

    private SellerProfile(Guid id, Guid userId, string companyName, Location location) : base(id)
    {
        UserId = userId;
        CompanyName = companyName;
        Location = location;
        IsVerified = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static SellerProfile Register(Guid userId, string companyName, Location location)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required.", nameof(companyName));

        var profile = new SellerProfile(Guid.NewGuid(), userId, companyName.Trim(), location);
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
