namespace Advertising.Application.Dtos;

// CampaignDto plus the product's title, resolved via IProductLookup at read time (same
// enrich-in-the-handler pattern as Billing's admin subscriptions list).
public sealed record MyCampaignRowDto(
    Guid Id,
    Guid ProductId,
    string ProductTitle,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? StoppedAt);
