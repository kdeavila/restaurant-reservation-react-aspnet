namespace RestaurantReservation.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string[] Errors { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
        Errors = string.IsNullOrEmpty(error) ? Array.Empty<string>() : new[] { error };
    }

    public static Result Succes() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);
}

public class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access Value of failed result");

    protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        _value = value;
    }
}