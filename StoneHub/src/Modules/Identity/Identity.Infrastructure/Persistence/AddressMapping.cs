using Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence;

// Column mapping for the optional owned Address, shared by all three profile tables so they
// store it the same way. All columns end up nullable (legacy profiles have no address).
internal static class AddressMapping
{
    public static void MapColumns<TProfile>(OwnedNavigationBuilder<TProfile, Address> address)
        where TProfile : class
    {
        address.Property(a => a.CountryCode).HasColumnName("AddressCountryCode").HasMaxLength(Address.CodeMaxLength);
        address.Property(a => a.RegionCode).HasColumnName("AddressRegionCode").HasMaxLength(Address.CodeMaxLength);
        address.Property(a => a.DistrictCode).HasColumnName("AddressDistrictCode").HasMaxLength(Address.CodeMaxLength);
        address.Property(a => a.Line1).HasColumnName("AddressLine1").HasMaxLength(Address.LineMaxLength);
        address.Property(a => a.Line2).HasColumnName("AddressLine2").HasMaxLength(Address.LineMaxLength);
        address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(Address.PostalCodeMaxLength);
        address.Property(a => a.Locality).HasColumnName("AddressLocality").HasMaxLength(Address.LocalityMaxLength);
    }
}
