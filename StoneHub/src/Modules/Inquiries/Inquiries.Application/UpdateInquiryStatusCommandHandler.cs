using Billing.Application;
using Inquiries.Domain;
using MediatR;
using SharedKernel;

namespace Inquiries.Application;

public sealed class UpdateInquiryStatusCommandHandler(IInquiryRepository inquiryRepository, IPlanEntitlements entitlements)
    : IRequestHandler<UpdateInquiryStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateInquiryStatusCommand request, CancellationToken ct)
    {
        var inquiry = await inquiryRepository.GetByIdAsync(request.InquiryId, ct);
        if (inquiry is null)
            return Result.NotFound("Inquiry not found.");
        if (inquiry.WholesalerId != request.CallerWholesalerId)
            return Result.Forbidden("You do not have permission to update this inquiry.");
        if (!await entitlements.CanUseOwnerFeaturesAsync(request.CallerWholesalerId, ct))
            return Result.PaymentRequired(InquiryPlanMessages.InquiriesNeedPlan);

        var result = request.Status.Trim().ToLowerInvariant() switch
        {
            "accepted" => inquiry.Accept(),
            "declined" => inquiry.Decline(),
            _ => Result.Failure($"Unknown status '{request.Status}'.")
        };
        if (result.IsFailure)
            return result;

        await inquiryRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
