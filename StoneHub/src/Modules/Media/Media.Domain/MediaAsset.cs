using SharedKernel;

namespace Media.Domain;

public sealed class MediaAsset : AggregateRoot<Guid>
{
    public Guid OwnerId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public MediaStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private MediaAsset() { }

    private MediaAsset(Guid id, Guid ownerId, string key, string contentType, string url) : base(id)
    {
        OwnerId = ownerId;
        Key = key;
        ContentType = contentType;
        Url = url;
        Status = MediaStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Id is caller-supplied because it drives the storage key.
    public static MediaAsset Create(Guid id, Guid ownerId, string key, string contentType, string url) =>
        new(id, ownerId, key, contentType, url);

    // Ownership is checked by the caller (the handler) before this is invoked.
    public void MarkUploaded()
    {
        if (Status == MediaStatus.Uploaded)
            return;

        Status = MediaStatus.Uploaded;
        Raise(new MediaUploadedEvent(Id, OwnerId, Key, Url));
    }
}
