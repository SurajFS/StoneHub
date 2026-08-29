namespace SharedKernel;

// A page of query results plus the metadata a client (e.g. an infinite scroll) needs to
// decide whether to fetch more, without inferring it from a short final page.
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
