namespace RestaurantReservation.Application.Common.Responses;

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Details { get; set; }
}

public abstract class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
