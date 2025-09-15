using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.UseCases.Reservations;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(
    CreateReservationUseCase createReservationUseCase,
    UpdateReservationUseCase updateReservationUseCase,
    IReservationService reservationService
) : ControllerBase
{
    private readonly CreateReservationUseCase _createReservationUseCase = createReservationUseCase;
    private readonly UpdateReservationUseCase _updateReservationUseCase = updateReservationUseCase;
    private readonly IReservationService _reservationService = reservationService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _reservationService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _reservationService.GetByIdAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _createReservationUseCase.ExecuteAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }   

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateReservationDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id) return BadRequest(new { error = "ID in URL does not match ID in body." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _updateReservationUseCase.ExecuteAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _reservationService.CancelReservationAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }
}