using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _userService.GetAllAsync(ct);
        return Ok(result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct = default)
    {
        var result = await _userService.DeactivateUserAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }
}