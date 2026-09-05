namespace SharedKernel;

public class Result
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    protected Result(
        bool isSuccess,
        string? error,
        ErrorType errorType,
        IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
        ValidationErrors = validationErrors ?? NoErrors;
    }

    public static Result Success() => new(true, null, ErrorType.None, null);
    public static Result<T> Success<T>(T value) => new(value, true, null, ErrorType.None, null);

    public static Result Failure(string error) => new(false, error, ErrorType.Failure, null);
    public static Result NotFound(string error) => new(false, error, ErrorType.NotFound, null);
    public static Result Conflict(string error) => new(false, error, ErrorType.Conflict, null);
    public static Result Unauthorized(string error) => new(false, error, ErrorType.Unauthorized, null);
    public static Result Forbidden(string error) => new(false, error, ErrorType.Forbidden, null);

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, "One or more validation errors occurred.", ErrorType.Validation, errors);

    public static Result<T> Failure<T>(string error) => new(default, false, error, ErrorType.Failure, null);
    public static Result<T> NotFound<T>(string error) => new(default, false, error, ErrorType.NotFound, null);
    public static Result<T> Conflict<T>(string error) => new(default, false, error, ErrorType.Conflict, null);
    public static Result<T> Unauthorized<T>(string error) => new(default, false, error, ErrorType.Unauthorized, null);
    public static Result<T> Forbidden<T>(string error) => new(default, false, error, ErrorType.Forbidden, null);

    public static Result<T> ValidationFailure<T>(IReadOnlyDictionary<string, string[]> errors) =>
        new(default, false, "One or more validation errors occurred.", ErrorType.Validation, errors);

    // Forward an existing failure into a differently-typed Result<T>, preserving its
    // classification (so an Unauthorized / Conflict doesn't collapse into a generic 400).
    public static Result<T> Failure<T>(Result failure) =>
        new(default, false, failure.Error, failure.ErrorType, failure.ValidationErrors);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    internal Result(
        T? value,
        bool isSuccess,
        string? error,
        ErrorType errorType,
        IReadOnlyDictionary<string, string[]>? validationErrors)
        : base(isSuccess, error, errorType, validationErrors) =>
        _value = value;
}
