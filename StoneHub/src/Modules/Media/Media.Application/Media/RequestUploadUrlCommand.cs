using Media.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Media.Application.Media;

public sealed record RequestUploadUrlCommand(Guid OwnerId, string FileName, string ContentType)
    : IRequest<Result<PresignUploadResult>>;
