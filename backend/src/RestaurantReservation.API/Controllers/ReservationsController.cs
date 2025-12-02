using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.UseCases.Reservations;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Policy = "AllRoles")]
public class ReservationsController(
    CreateReservationUseCase createReservationUseCase,
    UpdateReservationUseCase updateReservationUseCase,
    IReservationService reservationService)
    : ControllerBase
{
    private readonly CreateReservationUseCase _createReservationUseCase = createReservationUseCase;
    private readonly UpdateReservationUseCase _updateReservationUseCase = updateReservationUseCase;
    private readonly IReservationService _reservationService = reservationService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservationDto>>>> GetAll(
        [FromQuery] ReservationQueryParams queryParams,
        CancellationToken ct = default)
    {
        var (data, pagination) = await _reservationService.GetAllAsync(queryParams, ct);
        return Ok(ApiResponse<IEnumerable<ReservationDto>>.SuccessResponse(
            data, "Reservations retrieved successfully", pagination: pagination));
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
    public async Task<ActionResult<ApiResponse<ReservationDto>>> Update(
        int id, [FromBody] UpdateReservationDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<ReservationDto>.ErrorResponse(
                "ID mismatch", ErrorCodes.ValidationError, 400));

        var result = await _updateReservationUseCase.ExecuteAsync(dto, ct);

        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        var updatedResult = await _reservationService.GetByIdAsync(id, ct);

        if (updatedResult.IsFailure)
            return StatusCode(updatedResult.StatusCode,
                ApiResponse<ReservationDto>.ErrorResponse(
                    updatedResult.Error,
                    GetErrorCode(updatedResult.StatusCode),
                    updatedResult.StatusCode));

        return Ok(ApiResponse<ReservationDto>.SuccessResponse(
            updatedResult.Value, "Reservation updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct = default)
    {
        var result = await _reservationService.CancelAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ReservationDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<string>.SuccessResponse(result.Value, result.Value));
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