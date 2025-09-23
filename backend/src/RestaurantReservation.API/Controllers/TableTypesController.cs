using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/table-types")]
public class TableTypesController(ITableTypeService tableTypeService) : ControllerBase
{
    private readonly ITableTypeService _tableTypeService = tableTypeService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TableTypeDto>>>> GetAll(
        [FromQuery] TableTypeQueryParams queryParams,
        CancellationToken ct = default)
    {
        var (data, pagination) = await _tableTypeService.GetAllAsync(queryParams, ct);

        return Ok(ApiResponse<IEnumerable<TableTypeDto>>.SuccessResponse(
            data, "Table types retrieved successfully", pagination: pagination));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TableTypeDto>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await _tableTypeService.GetByIdAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<TableTypeDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<TableTypeDto>.SuccessResponse(result.Value, "Table type found"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TableTypeDto>>> Create(
        [FromBody] CreateTableTypeDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TableTypeDto>.ErrorResponse(
                "Invalid model", ErrorCodes.ValidationError, 400));

        var result = await _tableTypeService.CreateAsync(dto, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<TableTypeDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id },
            ApiResponse<TableTypeDto>.SuccessResponse(result.Value, "Table Type created", 201));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ApiResponse<TableTypeDto>>> Update(
        int id,
        [FromBody] UpdateTableTypeDto dto,
        CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest(ApiResponse<TableTypeDto>.ErrorResponse(
                "ID mismatch", ErrorCodes.ValidationError, 400));

        var result = await _tableTypeService.UpdateAsync(dto, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<TableTypeDto>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        var updatedResult = await _tableTypeService.GetByIdAsync(id, ct);
        if (updatedResult.IsFailure)
            return StatusCode(updatedResult.StatusCode, ApiResponse<TableTypeDto>.ErrorResponse(
                updatedResult.Error, GetErrorCode(updatedResult.StatusCode), updatedResult.StatusCode));

        return Ok(ApiResponse<TableTypeDto>.SuccessResponse(updatedResult.Value, "Table type updated"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct = default)
    {
        var result = await _tableTypeService.DeactivateAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<string>.ErrorResponse(
                result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<string>.SuccessResponse("Deletion completed", "Table type deleted"));
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