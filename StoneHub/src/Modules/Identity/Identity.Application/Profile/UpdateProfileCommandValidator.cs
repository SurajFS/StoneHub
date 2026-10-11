using FluentValidation;

namespace Identity.Application.Profile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        // `!` is safe: the rule only runs when Address is set (see When); it tells the compiler so.
        RuleFor(x => x.Address!).SetValidator(new ProfileAddressInputValidator()).When(x => x.Address is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).When(x => x.Address is null);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(20);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
    }
}
