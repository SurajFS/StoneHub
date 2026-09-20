namespace Advertising.Application.Dtos;

public sealed record CampaignDto(
    Guid Id,
    Guid ProductId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? StoppedAt);
