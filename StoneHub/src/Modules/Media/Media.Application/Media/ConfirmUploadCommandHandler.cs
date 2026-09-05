using Media.Application.Dtos;
using Media.Domain;
using MediatR;
using SharedKernel;

namespace Media.Application.Media;

public sealed class ConfirmUploadCommandHandler(IMediaAssetRepository repository)
    : IRequestHandler<ConfirmUploadCommand, Result<MediaAssetDto>>
{
    public async Task<Result<MediaAssetDto>> Handle(ConfirmUploadCommand request, CancellationToken ct)
    {
        var asset = await repository.GetByIdAsync(request.MediaId, ct);
        if (asset is null)
            return Result.NotFound<MediaAssetDto>("Media not found.");
        if (asset.OwnerId != request.OwnerId)
            return Result.Forbidden<MediaAssetDto>("You do not own this media.");

        asset.MarkUploaded();
        await repository.SaveChangesAsync(ct);

        return Result.Success(new MediaAssetDto(asset.Id, asset.Url, asset.ContentType, asset.Status.ToString()));
    }
}
