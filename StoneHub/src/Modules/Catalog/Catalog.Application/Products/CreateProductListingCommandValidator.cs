using FluentValidation;

namespace Catalog.Application.Products;

public sealed class CreateProductListingCommandValidator : AbstractValidator<CreateProductListingCommand>
{
    public CreateProductListingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.OwnerType).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.Size).MaximumLength(100);
        RuleFor(x => x.Thickness).MaximumLength(50);
        RuleFor(x => x.Finish).MaximumLength(50);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleForEach(x => x.Tags).MaximumLength(50);
        RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
