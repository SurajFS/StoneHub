using Identity.Domain;
using Xunit;

namespace Identity.Tests;

public sealed class ProfileAddressTests
{
    private static readonly Location Jaipur = Location.Create("Jaipur", "Rajasthan");
    private static readonly Location Kishangarh = Location.Create("Madanganj Kishangarh", "Rajasthan");

    private static Address AjmerAddress() =>
        Address.Create("IN", "IN-RJ", "IN-RJ-AJMER", "Plot 4, RIICO", null, "305801", "Madanganj Kishangarh");

    private static SellerProfile NewSeller()
    {
        var seller = SellerProfile.Register(Guid.NewGuid(), "Stone Co", Jaipur, null, "9876543210", null);
        seller.ClearDomainEvents();
        return seller;
    }

    [Fact]
    public void Address_TrimsAndBlanksOptionalFields()
    {
        var address = Address.Create(" IN ", "IN-RJ", "IN-RJ-AJMER", "  ", " Floor 2 ", "", null);

        Assert.Equal("IN", address.CountryCode);
        Assert.Null(address.Line1);
        Assert.Equal("Floor 2", address.Line2);
        Assert.Null(address.PostalCode);
        Assert.False(address.HasStreetAndPostalCode);
    }

    [Theory]
    [InlineData("", "IN-RJ", "IN-RJ-AJMER")]
    [InlineData("IN", " ", "IN-RJ-AJMER")]
    [InlineData("IN", "IN-RJ", "")]
    public void Address_RequiresCodes(string country, string region, string district) =>
        Assert.Throws<ArgumentException>(() => Address.Create(country, region, district, null, null, null, null));

    [Fact]
    public void Address_EqualByValue() => Assert.Equal(AjmerAddress(), AjmerAddress());

    [Theory]
    [InlineData("Jaipur", "Rajasthan", "Jaipur, Rajasthan")]
    [InlineData("Jaipur", null, "Jaipur")]
    [InlineData("Jaipur", " ", "Jaipur")]
    public void Location_Format(string city, string? state, string expected) =>
        Assert.Equal(expected, Location.Format(city, state));

    [Fact]
    public void UpdateProfile_LocationChange_RaisesOwnerProfileChanged()
    {
        var seller = NewSeller();

        seller.UpdateProfile("Stone Co", Kishangarh, AjmerAddress(), "9876543210", null, null);

        var changed = Assert.IsType<OwnerProfileChangedEvent>(Assert.Single(seller.DomainEvents));
        Assert.Equal(seller.UserId, changed.UserId);
        Assert.Equal("Madanganj Kishangarh, Rajasthan", changed.Location);
        Assert.Equal(AjmerAddress(), seller.Address);
    }

    [Fact]
    public void UpdateProfile_Rename_RaisesOwnerProfileChanged()
    {
        var seller = NewSeller();

        seller.UpdateProfile("Stone Co Pvt Ltd", Jaipur, null, "9876543210", null, null);

        Assert.Equal("Stone Co Pvt Ltd", Assert.IsType<OwnerProfileChangedEvent>(Assert.Single(seller.DomainEvents)).Name);
    }

    [Fact]
    public void UpdateProfile_PhoneOnly_RaisesNothing()
    {
        var seller = NewSeller();

        seller.UpdateProfile("Stone Co", Location.Create("Jaipur", "Rajasthan"), null, "9000000000", "9000000000", null);

        Assert.Empty(seller.DomainEvents);
    }

    [Fact]
    public void AttachAddress_SameDisplay_KeepsListingsQuiet()
    {
        var seller = NewSeller();

        seller.AttachAddress(Location.Create("Jaipur", "Rajasthan"), Address.Create("IN", "IN-RJ", "IN-RJ-JAIPUR", null, null, null, null));

        Assert.NotNull(seller.Address);
        Assert.Empty(seller.DomainEvents);
    }

    [Fact]
    public void Buyer_AttachAddress_SetsStateAndAddress()
    {
        var buyer = BuyerProfile.Register(Guid.NewGuid(), "Ramesh", BuyerType.Builder, "jaipur", null, null);

        buyer.AttachAddress("Jaipur", "Rajasthan", Address.Create("IN", "IN-RJ", "IN-RJ-JAIPUR", null, null, null, null));

        Assert.Equal("Jaipur", buyer.City);
        Assert.Equal("Rajasthan", buyer.State);
        Assert.Equal("IN-RJ-JAIPUR", buyer.Address?.DistrictCode);
    }
}
