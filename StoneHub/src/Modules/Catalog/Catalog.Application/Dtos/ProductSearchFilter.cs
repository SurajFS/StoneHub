namespace Catalog.Application.Dtos;

public sealed record ProductSearchFilter(
    Guid? CategoryId = null,
    Guid? SubcategoryId = null,
    string? Keyword = null,
    string? SellerName = null,
    string? Location = null,
    string? OwnerType = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool InStockOnly = false,
    decimal? MaxMinimumOrderQuantity = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20);
