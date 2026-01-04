using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll(
        [FromQuery] UserQueryParams queryParams,
        CancellationToken ct = default
    )
    {
        var (data, pagination) = await _userService.GetAllAsync(queryParams, ct);
        return Ok(
            ApiResponse<IEnumerable<UserDto>>.SuccessResponse(
                data,
                "Users retrieved successfully",
                pagination: pagination
            )
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(
        string id,
        CancellationToken ct = default
    )
    {
        var result = await _userService.GetByIdAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(
                result.StatusCode,
                ApiResponse<UserDto>.ErrorResponse(
                    result.Error,
                    GetErrorCode(result.StatusCode),
                    result.StatusCode
                )
            );

        return Ok(ApiResponse<UserDto>.SuccessResponse(result.Value, "User found"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> Deactivate(
        string id,
        CancellationToken ct = default
    )
    {
        var result = await _userService.DeactivateAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(
                result.StatusCode,
                ApiResponse<string>.ErrorResponse(
                    result.Error,
                    GetErrorCode(result.StatusCode),
                    result.StatusCode
                )
            );

        return Ok(ApiResponse<string>.SuccessResponse(result.Value, result.Value));
    }

    private static string GetErrorCode(int statusCode) =>
        statusCode switch
        {
            400 => ErrorCodes.ValidationError,
            404 => ErrorCodes.NotFound,
            401 => ErrorCodes.Unauthorized,
            409 => ErrorCodes.Conflict,
            422 => ErrorCodes.BusinessRuleViolation,
            _ => ErrorCodes.InternalError,
        };
}
