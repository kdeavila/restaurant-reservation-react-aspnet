using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tables")]
public class TablesController(ITableService tableService) : ControllerBase
{
   private readonly ITableService _tableService = tableService;

   [HttpGet]
   [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false)]
   public async Task<ActionResult<ApiResponse<IEnumerable<TableDetailedDto>>>> GetAll(
       [FromQuery] TableQueryParams queryParams, CancellationToken ct = default)
   {
      var (data, pagination) = await _tableService.GetAllAsync(queryParams, ct);

      return Ok(ApiResponse<IEnumerable<TableDetailedDto>>.SuccessResponse(
          data, "Tables retrieved successfully", pagination: pagination));
   }

   [HttpGet("{id:int}")]
   [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false)]
   public async Task<ActionResult<ApiResponse<TableDetailedDto>>> GetById(int id, CancellationToken ct = default)
   {
      var result = await _tableService.GetByIdAsync(id, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<TableDetailedDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      return Ok(ApiResponse<TableDetailedDto>.SuccessResponse(result.Value, "Table found"));
   }

   [HttpPost]
   [Authorize(Policy = "AdminOrManager")]
   public async Task<ActionResult<ApiResponse<TableDetailedDto>>> Create([FromBody] CreateTableDto dto,
       CancellationToken ct = default)
   {
      if (!ModelState.IsValid)
         return BadRequest(ApiResponse<TableDetailedDto>.ErrorResponse(
             "Invalid model", ErrorCodes.ValidationError));

      var result = await _tableService.CreateAsync(dto, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<TableDetailedDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      return CreatedAtAction(nameof(GetById), new { id = result.Value.Id },
          ApiResponse<TableDetailedDto>.SuccessResponse(result.Value, "Table created"));
   }

   [HttpPatch("{id:int}")]
   [Authorize(Policy = "AdminOrManager")]
   public async Task<ActionResult<ApiResponse<TableDetailedDto>>> Update(
       int id,
       [FromBody] UpdateTableDto dto,
       CancellationToken ct = default)
   {
      if (id != dto.Id)
         return BadRequest(ApiResponse<TableDetailedDto>
             .ErrorResponse("ID mismatch", ErrorCodes.ValidationError));

      var result = await _tableService.UpdateAsync(dto, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<TableDetailedDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      var updatedResult = await _tableService.GetByIdAsync(id, ct);
      if (result.IsFailure)
         return StatusCode(updatedResult.StatusCode, ApiResponse<TableDetailedDto>
             .ErrorResponse(updatedResult.Error, GetErrorCode(updatedResult.StatusCode), updatedResult.StatusCode));

      return Ok(ApiResponse<TableDetailedDto>.SuccessResponse(updatedResult.Value, "Table updated"));
   }

   [HttpDelete("{id:int}")]
   [Authorize(Policy = "AdminOnly")]
   public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct = default)
   {
      var result = await _tableService.DeactivateAsync(id, ct);

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