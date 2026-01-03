using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.UseCases.PricingRules;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/pricing-rules")]
public class PricingRulesController(
    IPricingRuleService pricingRuleService,
    CreatePricingRuleUseCase createPricingRuleUseCase
) : ControllerBase
{
   private readonly IPricingRuleService _pricingRuleService = pricingRuleService;
   private readonly CreatePricingRuleUseCase _createPricingRuleUseCase = createPricingRuleUseCase;

   [HttpGet]
   [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, NoStore = false)]
   public async Task<ActionResult<ApiResponse<IEnumerable<PricingRuleDto>>>> GetAll(
       [FromQuery] PricingRuleQueryParams queryParams, CancellationToken ct = default)
   {
      var (data, pagination) = await _pricingRuleService.GetAllAsync(queryParams, ct);

      return Ok(ApiResponse<IEnumerable<PricingRuleDto>>
          .SuccessResponse(data, "Pricing rules retrieved successfully", pagination: pagination));
   }

   [HttpGet("{id:int}")]
   [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, NoStore = false)]
   public async Task<ActionResult<ApiResponse<PricingRuleDto>>> GetById(int id, CancellationToken ct = default)
   {
      var result = await _pricingRuleService.GetByIdAsync(id, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<PricingRuleDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      return Ok(ApiResponse<PricingRuleDto>.SuccessResponse(result.Value, "Pricing rule found"));
   }

   [HttpPost]
   [Authorize(Policy = "AdminOrManager")]
   public async Task<ActionResult<ApiResponse<PricingRuleDto>>> Create(
       [FromBody] CreatePricingRuleDto dto, CancellationToken ct = default)
   {
      if (!ModelState.IsValid)
         return BadRequest(ApiResponse<PricingRuleDto>
             .ErrorResponse("Invalid model", ErrorCodes.ValidationError));

      var result = await _createPricingRuleUseCase.ExecuteAsync(dto, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<PricingRuleDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      return CreatedAtAction(nameof(GetById), new { id = result.Value.Id },
          ApiResponse<PricingRuleDto>.SuccessResponse(result.Value, "Pricing rule created", 201));
   }

   [HttpPatch("{id:int}")]
   [Authorize(Policy = "AdminOrManager")]
   public async Task<ActionResult<ApiResponse<PricingRuleDto>>> Update(
       int id, UpdatePricingRuleDto dto, CancellationToken ct = default)
   {
      if (id != dto.Id)
         return BadRequest(ApiResponse<PricingRuleDto>.ErrorResponse("ID mismatch", ErrorCodes.ValidationError));

      var result = await _pricingRuleService.UpdateAsync(dto, ct);
      if (result.IsFailure)
         return StatusCode(result.StatusCode, ApiResponse<PricingRuleDto>
             .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

      var updatedResult = await _pricingRuleService.GetByIdAsync(id, ct);
      if (updatedResult.IsFailure)
         return StatusCode(updatedResult.StatusCode, ApiResponse<PricingRuleDto>
             .ErrorResponse(updatedResult.Error, GetErrorCode(updatedResult.StatusCode), updatedResult.StatusCode));

      return Ok(ApiResponse<PricingRuleDto>.SuccessResponse(updatedResult.Value, "Pricing rule updated"));
   }

   [HttpDelete("{id:int}")]
   [Authorize(Policy = "AdminOnly")]
   public async Task<ActionResult<ApiResponse<string>>> Delete(int id, CancellationToken ct = default)
   {
      var result = await _pricingRuleService.DeactivateAsync(id, ct);
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