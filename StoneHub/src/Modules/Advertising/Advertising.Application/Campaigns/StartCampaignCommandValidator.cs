using FluentValidation;

namespace Advertising.Application.Campaigns;

public sealed class StartCampaignCommandValidator : AbstractValidator<StartCampaignCommand>
{
    public StartCampaignCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
