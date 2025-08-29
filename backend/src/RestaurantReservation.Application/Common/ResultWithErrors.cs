namespace RestaurantReservation.Application.Common;

public class ResultWithErrors : Result
{
    public new IEnumerable<string> Errors { get; }

    protected ResultWithErrors(bool isSuccess, IEnumerable<string> errors)
        : base(isSuccess, errors.FirstOrDefault() ?? string.Empty)
    {
        Errors = errors;
    }

    public static ResultWithErrors Failure(IEnumerable<string> errors) =>
        new ResultWithErrors(false, errors);
}

public class ResultWithErrors<T> : Result<T>
{
    public new IEnumerable<string> Errors { get; }

    protected ResultWithErrors(T value, bool isSuccess, IEnumerable<string> errors)
        : base(value, isSuccess, errors.FirstOrDefault() ?? string.Empty)
    {
        Errors = errors;
    }

    public static ResultWithErrors<T> Failure(IEnumerable<string> errors) =>
        new ResultWithErrors<T>(default!, false, errors);
}