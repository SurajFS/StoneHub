using Identity.Application.Profile;
using Identity.Domain;
using Identity.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel;
using Xunit;

namespace Identity.Tests;

public sealed class ProfileLocationResolverTests
{
    private readonly FakeLocationDirectory _directory = new();

    private ProfileLocationResolver Resolver => new(_directory, NullLogger<ProfileLocationResolver>.Instance);

    private static ProfileAddressInput Ajmer(string? line1 = "Plot 4, RIICO", string? pin = FakeLocationDirectory.AjmerPin, string? locality = null) =>
        new("IN", "IN-RJ", FakeLocationDirectory.AjmerDistrict, line1, "Shed B", pin, locality);

    private static ProfileLocationRequest Structured(ProfileAddressInput address, AddressRequirement requirement) =>
        new("Seller", requirement, address, null, null, null, null, null);

    private static ProfileLocationRequest Legacy(string city, string? state, string? currentCity = null, string? currentState = null, Address? current = null) =>
        new("Seller", AddressRequirement.StreetAndPostalCode, null, city, state, currentCity, currentState, current);

    [Fact]
    public async Task Structured_WithLocality_DisplaysLocality()
    {
        var result = await Resolver.ResolveAsync(Structured(Ajmer(locality: "madanganj kishangarh"), AddressRequirement.StreetAndPostalCode));

        Assert.True(result.IsSuccess);
        Assert.Equal(FakeLocationDirectory.Kishangarh, result.Value.City);
        Assert.Equal("Rajasthan", result.Value.State);
        Assert.Equal(FakeLocationDirectory.Kishangarh, result.Value.Address?.Locality);
        Assert.Equal("Shed B", result.Value.Address?.Line2);
    }

    [Fact]
    public async Task Structured_WithoutLocality_DisplaysDistrict()
    {
        var result = await Resolver.ResolveAsync(Structured(Ajmer(pin: null, line1: null), AddressRequirement.DistrictOnly));

        Assert.True(result.IsSuccess);
        Assert.Equal("Ajmer", result.Value.City);
    }

    [Fact]
    public async Task Owner_MissingStreetAndPin_FieldErrorsWithoutCallingDirectory()
    {
        var result = await Resolver.ResolveAsync(Structured(Ajmer(line1: " ", pin: null), AddressRequirement.StreetAndPostalCode));

        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Equal(["Address.Line1", "Address.PostalCode"], result.ValidationErrors.Keys.Order());
        Assert.Equal(0, _directory.ResolveCalls);
    }

    [Fact]
    public async Task DirectoryRejection_IsKeyedUnderAddress()
    {
        var result = await Resolver.ResolveAsync(Structured(Ajmer(pin: "302001"), AddressRequirement.StreetAndPostalCode));

        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Equal("Address.PostalCode", Assert.Single(result.ValidationErrors).Key);
    }

    [Fact]
    public async Task Legacy_NewProfile_StoresFreeTextWithoutAddress()
    {
        var result = await Resolver.ResolveAsync(Legacy(" Kishangarh ", "Rajasthan"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Kishangarh", result.Value.City);
        Assert.Null(result.Value.Address);
    }

    [Fact]
    public async Task Legacy_UnchangedText_KeepsStructuredAddress()
    {
        var current = Address.Create("IN", "IN-RJ", FakeLocationDirectory.AjmerDistrict, "Plot 4", null, "305801", null);

        var result = await Resolver.ResolveAsync(Legacy("ajmer", "RAJASTHAN", "Ajmer", "Rajasthan", current));

        Assert.Same(current, result.Value.Address);
    }

    [Fact]
    public async Task Legacy_EditedText_DropsStaleAddress()
    {
        var current = Address.Create("IN", "IN-RJ", FakeLocationDirectory.AjmerDistrict, "Plot 4", null, "305801", null);

        var result = await Resolver.ResolveAsync(Legacy("Jaipur", "Rajasthan", "Ajmer", "Rajasthan", current));

        Assert.Equal("Jaipur", result.Value.City);
        Assert.Null(result.Value.Address);
    }

    [Fact]
    public async Task Legacy_BlankCity_IsAFieldError()
    {
        var result = await Resolver.ResolveAsync(Legacy(" ", "Rajasthan"));

        Assert.Equal("City", Assert.Single(result.ValidationErrors).Key);
    }
}
