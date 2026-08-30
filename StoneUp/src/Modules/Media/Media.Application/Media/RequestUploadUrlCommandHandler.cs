using Media.Application.Dtos;
using Media.Domain;
using MediatR;
using SharedKernel;

namespace Media.Application.Media;

public sealed class RequestUploadUrlCommandHandler(
    IMediaAssetRepository repository,
    IStorageService storageService) : IRequestHandler<RequestUploadUrlCommand, Result<PresignUploadResult>>
{
    public async Task<Result<PresignUploadResult>> Handle(RequestUploadUrlCommand request, CancellationToken ct)
    {
        if (!storageService.IsConfigured)
            return Result.Failure<PresignUploadResult>("Media storage is not configured.");

        var mediaId = Guid.NewGuid();
        var extension = MediaContentType.ExtensionFor(request.ContentType);
        var key = $"uploads/{request.OwnerId}/{mediaId}{extension}";
        var publicUrl = storageService.GetPublicUrl(key);

        var asset = MediaAsset.Create(mediaId, request.OwnerId, key, request.ContentType, publicUrl);
        repository.Add(asset);
        await repository.SaveChangesAsync(ct);

        var uploadUrl = await storageService.GeneratePresignedUploadUrlAsync(key, request.ContentType, ct);
        return Result.Success(new PresignUploadResult(mediaId, key, uploadUrl, publicUrl));
    }
}
