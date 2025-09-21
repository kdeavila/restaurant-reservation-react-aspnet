using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Exceptions;

namespace RestaurantReservation.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException vex => ApiResponse<object>.ErrorResponse(
                vex.Message,
                ErrorCodes.ValidationError),

            NotFoundException nex => ApiResponse<object>.ErrorResponse(
                nex.Message,
                ErrorCodes.NotFound,
                StatusCodes.Status404NotFound),

            UnauthorizedAccessException uex => ApiResponse<object>.ErrorResponse(
                uex.Message,
                ErrorCodes.Unauthorized,
                StatusCodes.Status401Unauthorized),

            ConflictException cex => ApiResponse<object>.ErrorResponse(
                cex.Message,
                ErrorCodes.Conflict,
                StatusCodes.Status409Conflict
            ),

            BusinessRuleException brex => ApiResponse<object>.ErrorResponse(
                brex.Message,
                ErrorCodes.BusinessRuleViolation,
                StatusCodes.Status422UnprocessableEntity),

            _ => ApiResponse<object>.ErrorResponse(
                "An unexpected error occurred",
                ErrorCodes.InternalError,
                StatusCodes.Status500InternalServerError)
        };

        context.Response.StatusCode = response.StatusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}