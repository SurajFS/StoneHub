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
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join(" ", failures.Select(f => f.ErrorMessage));

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(errorMessage);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(string)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [errorMessage])!;
    }
}
