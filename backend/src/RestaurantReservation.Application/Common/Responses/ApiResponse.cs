using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.Common.Responses;

namespace RestaurantReservation.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public ApiError? Error { get; set; }
    public PaginationMetadata? Pagination { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string errorCode, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Error = new ApiError
            {
                Code = errorCode,
                Message = message
            }
        };
    }
}