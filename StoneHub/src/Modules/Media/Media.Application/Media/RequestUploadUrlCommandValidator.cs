using FluentValidation;

namespace Media.Application.Media;

public sealed class RequestUploadUrlCommandValidator : AbstractValidator<RequestUploadUrlCommand>
{
    public RequestUploadUrlCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(MediaContentType.IsAllowed)
            .WithMessage("Unsupported file type. Allowed: JPEG, PNG, WebP, MP4.");
    }
}
