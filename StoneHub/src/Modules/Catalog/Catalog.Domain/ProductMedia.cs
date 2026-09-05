using SharedKernel;

namespace Catalog.Domain;

public sealed class ProductMedia : ValueObject
{
    public string Url { get; }
    public MediaType MediaType { get; }

    private ProductMedia(string url, MediaType mediaType)
    {
        Url = url;
        MediaType = mediaType;
    }

    public static ProductMedia Create(string url, MediaType mediaType)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url is required.", nameof(url));
        return new ProductMedia(url, mediaType);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Url;
        yield return MediaType;
    }
}
