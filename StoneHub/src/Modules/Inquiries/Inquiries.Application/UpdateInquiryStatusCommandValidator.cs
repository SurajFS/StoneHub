using FluentValidation;

namespace Inquiries.Application;

public sealed class UpdateInquiryStatusCommandValidator : AbstractValidator<UpdateInquiryStatusCommand>
{
    public UpdateInquiryStatusCommandValidator()
    {
        RuleFor(x => x.InquiryId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
    }
}
