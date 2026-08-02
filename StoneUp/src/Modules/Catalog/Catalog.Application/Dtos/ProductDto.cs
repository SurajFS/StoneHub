namespace Catalog.Application.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid SellerId,
    string Title,
    string MaterialType,
    string Size,
    string Thickness,
    string Finish,
    decimal QuantityAvailable,
    decimal Price,
    string Currency,
    bool IsAvailable,
    IReadOnlyList<string> MediaUrls,
    DateTimeOffset CreatedAt);
