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
[Route("api/v{version:apiVersion}/auth")]
[AllowAnonymous]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(
        [FromBody] CreateUserDto dto,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(
                ApiResponse<UserDto>.ErrorResponse(
                    "Invalid model state.",
                    ErrorCodes.ValidationError,
                    400
                )
            );

        var result = await _userService.RegisterAsync(dto, ct);
        if (result.IsFailure)
        {
            var status = result.StatusCode == 0 ? 400 : result.StatusCode;
            var errorCode = GetErrorCode(status);
            var response = ApiResponse<UserDto>.ErrorResponse(result.Error, errorCode, status);

            return StatusCode(status, response);
        }

        var success = ApiResponse<UserDto>.SuccessResponse(
            result.Value,
            "User registered successfully",
            201
        );
        return Created($"/api/users/{result.Value.Id}", success);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthDto>>> Login(
        [FromBody] LoginDto dto,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(
                ApiResponse<AuthDto>.ErrorResponse("Invalid model state.", "VALIDATION_ERROR", 400)
            );

        var result = await _userService.LoginAsync(dto, ct);
        if (result.IsFailure)
        {
            var status = result.StatusCode == 0 ? 401 : result.StatusCode;
            var errorCode = GetErrorCode(status);
            var response = ApiResponse<AuthDto>.ErrorResponse(result.Error, errorCode, status);
            return StatusCode(status, response);
        }

        var success = ApiResponse<AuthDto>.SuccessResponse(result.Value, "Login successful", 200);
        return Ok(success);
    }

    private static string GetErrorCode(int statusCode)
    {
        return statusCode switch
        {
            400 => ErrorCodes.ValidationError,
            401 => ErrorCodes.Unauthorized,
            403 => ErrorCodes.AuthError,
            404 => ErrorCodes.NotFound,
            409 => ErrorCodes.Conflict,
            500 => ErrorCodes.InternalError,
            _ => ErrorCodes.InternalError,
        };
    }
}
