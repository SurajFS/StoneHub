using FluentValidation;

namespace Catalog.Application.Products;

public sealed class CreateProductListingCommandValidator : AbstractValidator<CreateProductListingCommand>
{
    public CreateProductListingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaterialType).NotEmpty();
        RuleFor(x => x.Size).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Thickness).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Finish).NotEmpty().MaximumLength(50);
        RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
