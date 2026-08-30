namespace Media.Application.Dtos;

public sealed record MediaAssetDto(Guid Id, string Url, string ContentType, string Status);
