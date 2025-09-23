namespace RestaurantReservation.Application.Common;

public class Result
{
    protected bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public int StatusCode { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool isSuccess, IEnumerable<string> errors, int statusCode)
    {
        var errorList = errors as string[] ?? errors.ToArray();

        if (isSuccess && errorList.Any())
            throw new InvalidOperationException("Success result cannot have errors");

        if (!isSuccess && !errorList.Any())
            throw new InvalidOperationException("Failure result must have errors");

        IsSuccess = isSuccess;
        Errors = errorList.ToArray();
        Error = errorList.FirstOrDefault() ?? string.Empty;
        StatusCode = statusCode;
    }

    // Static methods for Result
    public static Result Success()
        => new Result(true, [], 200);

    public static Result Failure(string error, int statusCode)
        => new Result(false, [error], statusCode);

    public static Result Failure(IEnumerable<string> errors, int statusCode)
        => new Result(false, errors, statusCode);

    // Generic methods for Result
    public static Result<T> Success<T>(T value)
        => new Result<T>(value, true, [], 200);

    public static Result<T> Failure<T>(string error, int statusCode)
        => new Result<T>(default!, false, [error], statusCode);

    public static Result<T> Failure<T>(IEnumerable<string> errors, int statusCode)
        => new Result<T>(default!, false, errors, statusCode);
}

public class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access Value of failed result");

    protected internal Result(T value, bool isSuccess, IEnumerable<string> errors, int statusCode)
        : base(isSuccess, errors, statusCode) => _value = value;
}