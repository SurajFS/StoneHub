using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace StoneUp.Api;

// The single place that turns a Result failure into an HTTP status + ProblemDetails, so no
// controller re-decides that mapping. Success shaping (200 vs 201 vs 204) stays in the
// controller, where the resource semantics live — controllers call ToActionResult() only on
// the failure branch, except the generic overload which also returns the value on success.
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result) =>
        result.IsSuccess ? new OkResult() : Problem(result);

    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : Problem(result);

    private static IActionResult Problem(Result result)
    {
        if (result.ErrorType == ErrorType.Validation)
        {
            var validationProblem = new ValidationProblemDetails(
                result.ValidationErrors.ToDictionary(entry => entry.Key, entry => entry.Value))
            {
                Status = StatusCodes.Status400BadRequest
            };
            return new BadRequestObjectResult(validationProblem);
        }

        var status = StatusFor(result.ErrorType);
        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleFor(result.ErrorType),
            Detail = result.Error
        };
        return new ObjectResult(problem) { StatusCode = status };
    }

    private static int StatusFor(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    };

    private static string TitleFor(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => "Resource not found.",
        ErrorType.Conflict => "Conflict.",
        ErrorType.Unauthorized => "Unauthorized.",
        ErrorType.Forbidden => "Forbidden.",
        _ => "Bad request."
    };
}
