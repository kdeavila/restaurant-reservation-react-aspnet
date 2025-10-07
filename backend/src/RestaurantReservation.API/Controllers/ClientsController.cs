using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientDto>>>> GetAll(
        [FromQuery] ClientQueryParams queryParams, CancellationToken ct = default)
    {
        var (data, pagination) = await _clientService.GetAllAsync(queryParams, ct);
        return Ok(ApiResponse<IEnumerable<ClientDto>>.SuccessResponse
            (data, "Clients retrieved successfully", pagination: pagination));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await _clientService.GetByIdAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode,
                ApiResponse<ClientDto>.ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<ClientDto>.SuccessResponse(result.Value, "Client found"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Create([FromBody] CreateClientDto dto,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ClientDto>.ErrorResponse(
                "Invalid model", ErrorCodes.ValidationError));

        var result = await _clientService.CreateClientAsync(dto, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ClientDto>
                .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id },
            ApiResponse<ClientDto>.SuccessResponse(result.Value, "Client created", 201));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Update
        (int id, [FromBody] UpdateClientDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id) return BadRequest("ID in URL does not match ID in body.");

        var result = await _clientService.UpdateClientAsync(dto, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<ClientDto>
                .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        var updatedResult = await _clientService.GetByIdAsync(id, ct);
        if (updatedResult.IsFailure)
            return StatusCode(updatedResult.StatusCode, ApiResponse<ClientDto>
                .ErrorResponse(updatedResult.Error, GetErrorCode(updatedResult.StatusCode), updatedResult.StatusCode));

        return Ok(ApiResponse<ClientDto>.SuccessResponse(updatedResult.Value, "Client updated"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct = default)
    {
        var result = await _clientService.DeactivateClientAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<string>
                .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

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