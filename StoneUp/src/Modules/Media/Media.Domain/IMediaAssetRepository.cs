namespace Media.Domain;

public interface IMediaAssetRepository
{
    void Add(MediaAsset asset);
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
