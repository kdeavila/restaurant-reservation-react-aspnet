using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        var result = await _userService.RegisterUserAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _userService.LoginAsync(dto, ct);
        return result.IsFailure
            ? Unauthorized(new { error = result.Error })
            : Ok(result.Value);
    }
}