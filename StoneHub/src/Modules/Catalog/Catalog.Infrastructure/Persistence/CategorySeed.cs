namespace Catalog.Infrastructure.Persistence;

// Stable ids for the seeded top-level categories. Fixed so migrations are deterministic and
// later work (e.g. backfilling existing Marble/Granite products) can reference them directly.
internal static class CategorySeed
{
    public static readonly Guid Marble = new("11111111-1111-1111-1111-000000000001");
    public static readonly Guid Granite = new("11111111-1111-1111-1111-000000000002");
    public static readonly Guid Tiles = new("11111111-1111-1111-1111-000000000003");
    public static readonly Guid KotaStone = new("11111111-1111-1111-1111-000000000004");
    public static readonly Guid Handicrafts = new("11111111-1111-1111-1111-000000000005");
}
