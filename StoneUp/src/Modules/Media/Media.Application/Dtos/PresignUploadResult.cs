namespace Media.Application.Dtos;

public sealed record PresignUploadResult(Guid MediaId, string Key, string UploadUrl, string PublicUrl);
