namespace RestaurantReservation.Application.Common;

public class Result
{
    protected bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        var errorList = errors as string[] ?? errors.ToArray();
        if (isSuccess && errorList.Any()) 
            throw new InvalidOperationException("Success result cannot have errors");

        if (!isSuccess && !errorList.Any())
            throw new InvalidOperationException("Failure result must have errors");

        IsSuccess = isSuccess;
        Errors = errorList.ToArray();
        Error = errorList.FirstOrDefault() ?? string.Empty;
    }

    public static Result Success() => new Result(true, []);
    public static Result Failure(string error) => new Result(false, [error]);
    public static Result Failure(IEnumerable<string> errors) => new Result(false, errors);

    public static Result<T> Success<T>(T value) => new Result<T>(value, true, []);
    public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, [error]);
    public static Result<T> Failure<T>(IEnumerable<string> errors) => new Result<T>(default!, false, errors);
}

public class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access Value of failed result");

    protected internal Result(T value, bool isSuccess, IEnumerable<string> errors)
        : base(isSuccess, errors) => _value = value;
}