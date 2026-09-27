using Catalog.Domain;

namespace Catalog.Tests;

internal static class ProductTestData
{
    public static Product Listing(Guid sellerId) =>
        Product.CreateListing(
            sellerId, ListingOwnerType.Seller, "Carrara Marble Slab", Guid.NewGuid(), subcategoryId: null,
            size: null, thickness: null, finish: null, color: null, unit: ProductUnit.Slab, tags: [],
            quantityAvailable: 10, price: Money.Create(185m), wholesalePrice: null, minimumOrderQuantity: null).Value;
}
