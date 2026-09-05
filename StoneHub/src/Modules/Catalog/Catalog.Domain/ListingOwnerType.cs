namespace Catalog.Domain;

// Who owns the listing. Denormalized onto the product so search/UI can distinguish retail from
// wholesale supply without crossing into the Identity module.
public enum ListingOwnerType
{
    Seller,
    Wholesaler
}
