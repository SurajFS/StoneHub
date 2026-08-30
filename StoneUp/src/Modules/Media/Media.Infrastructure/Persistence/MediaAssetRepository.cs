using Media.Domain;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Persistence;

public sealed class MediaAssetRepository(MediaDbContext dbContext) : IMediaAssetRepository
{
    public void Add(MediaAsset asset) => dbContext.MediaAssets.Add(asset);

    public Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.MediaAssets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
