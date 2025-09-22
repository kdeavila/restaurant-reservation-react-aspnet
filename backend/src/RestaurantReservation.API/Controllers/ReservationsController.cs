using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.UseCases.Reservations;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly CreateReservationUseCase _createReservationUseCase;
    private readonly UpdateReservationUseCase _updateReservationUseCase;
    private readonly IReservationService _reservationService;

    public ReservationsController(
        CreateReservationUseCase createReservationUseCase,
        UpdateReservationUseCase updateReservationUseCase,
        IReservationService reservationService)
    {
        _createReservationUseCase = createReservationUseCase;
        _updateReservationUseCase = updateReservationUseCase;
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservationDto>>>> GetAll(
        [FromQuery] ReservationQueryParams query,
        CancellationToken ct = default)
    {
        var (data, pagination) = await _reservationService.GetAllAsync(query, ct);
        return Ok(ApiResponse<IEnumerable<ReservationDto>>.SuccessResponse(
            data, "Reservations retrieved successfully", 200, pagination));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await _reservationService.GetByIdAsync(id, ct);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<ReservationDto>.SuccessResponse(result.Value, "Reservation found"));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CreateReservation(
        [FromBody] CreateReservationDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(
                ApiResponse<ReservationDto>.ErrorResponse("Invalid data", ErrorCodes.ValidationError));

        var result = await _createReservationUseCase.ExecuteAsync(dto, ct);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return CreatedAtAction(nameof(GetById),
            new { id = result.Value.Id },
            ApiResponse<ReservationDto>.SuccessResponse(result.Value, "Reservation created", 201));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        int id, [FromBody] UpdateReservationDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<ReservationDto>.ErrorResponse("ID in URL does not match ID in body",
                ErrorCodes.ValidationError));

        if (!ModelState.IsValid)
            return BadRequest(
                ApiResponse<ReservationDto>.ErrorResponse("Invalid data", ErrorCodes.ValidationError));

        var result = await _updateReservationUseCase.ExecuteAsync(dto, ct);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct = default)
    {
        var result = await _reservationService.CancelReservationAsync(id, ct);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return NoContent();
    }

    private static string GetErrorCode(int statusCode) => statusCode switch
    {
        400 => ErrorCodes.ValidationError,
        404 => ErrorCodes.NotFound,
        401 => ErrorCodes.Unauthorized,
        409 => ErrorCodes.Conflict,
        422 => ErrorCodes.BusinessRuleViolation,
        _ => ErrorCodes.InternalError
    };
}