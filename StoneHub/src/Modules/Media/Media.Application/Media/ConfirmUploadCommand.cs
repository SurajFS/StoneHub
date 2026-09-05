using Media.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Media.Application.Media;

public sealed record ConfirmUploadCommand(Guid MediaId, Guid OwnerId) : IRequest<Result<MediaAssetDto>>;
