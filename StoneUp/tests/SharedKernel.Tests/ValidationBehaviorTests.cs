using FluentValidation;
using MediatR;
using Xunit;

namespace SharedKernel.Tests;

public sealed class ValidationBehaviorTests
{
    private sealed record SampleCommand(string Name, int Age) : IRequest<Result<string>>;

    private sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Age).GreaterThan(0);
        }
    }

    private static ValidationBehavior<SampleCommand, Result<string>> BehaviorWithValidator() =>
        new([new SampleCommandValidator()]);

    private static ValidationBehavior<SampleCommand, Result<string>> BehaviorWithoutValidators() =>
        new([]);

    private static RequestHandlerDelegate<Result<string>> NextReturning(string value) =>
        () => Task.FromResult(Result.Success(value));

    [Fact]
    public async Task Handle_WithInvalidInput_ReturnsFieldGroupedValidationFailure()
    {
        var behavior = BehaviorWithValidator();
        var command = new SampleCommand(Name: "", Age: 0);

        var result = await behavior.Handle(command, NextReturning("unreached"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Contains(nameof(SampleCommand.Name), result.ValidationErrors.Keys);
        Assert.Contains(nameof(SampleCommand.Age), result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_WithValidInput_CallsNext()
    {
        var behavior = BehaviorWithValidator();
        var command = new SampleCommand(Name: "Ada", Age: 30);

        var result = await behavior.Handle(command, NextReturning("ok"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = BehaviorWithoutValidators();
        var command = new SampleCommand(Name: "", Age: 0);

        var result = await behavior.Handle(command, NextReturning("ok"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
    }
}
