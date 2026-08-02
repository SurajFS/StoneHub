namespace Catalog.Application.Dtos;

public sealed record ProductSearchFilter(
    string? MaterialType = null,
    string? Keyword = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 20);
