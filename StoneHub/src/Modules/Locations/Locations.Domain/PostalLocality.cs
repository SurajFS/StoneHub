using SharedKernel;

namespace Locations.Domain;

// One locality or town served by a postal code (an India Post office area). A postal code can
// cover several localities, and occasionally spans two districts. Loaded in bulk by the importer.
public sealed class PostalLocality : Entity<long>
{
    public string CountryId { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string DistrictId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private PostalLocality() { }
}
