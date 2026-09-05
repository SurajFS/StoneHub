using FluentValidation;
using MediatR;

namespace SharedKernel;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(f => f.ErrorMessage).Distinct().ToArray());

        return CreateValidationFailure(errors);
    }

    // Commands return Result; queries with validators return Result<T>. Build the right
    // failure shape without the caller having to know which one this pipeline serves.
    private static TResponse CreateValidationFailure(IReadOnlyDictionary<string, string[]> errors)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.ValidationFailure(errors);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.ValidationFailure), 1, [typeof(IReadOnlyDictionary<string, string[]>)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [errors])!;
    }
}
