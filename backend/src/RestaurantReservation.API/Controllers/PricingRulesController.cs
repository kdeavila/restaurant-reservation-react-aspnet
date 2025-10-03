using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.UseCases.PricingRules;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/pricing-rules")]
public class PricingRulesController(
    IPricingRuleService pricingRuleService,
    CreatePricingRuleUseCase createPricingRuleUseCase
) : ControllerBase
{
    private readonly IPricingRuleService _pricingRuleService = pricingRuleService;
    private readonly CreatePricingRuleUseCase _createPricingRuleUseCase = createPricingRuleUseCase;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PricingRuleDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await _pricingRuleService.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<PricingRuleDto>>.SuccessResponse(result,
            "Pricing rules retrieved successfully"));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PricingRuleDto>>> GetById(int id, CancellationToken ct = default)
    {
        var result = await _pricingRuleService.GetByIdAsync(id, ct);
        if (result.IsFailure)
            return StatusCode(result.StatusCode, ApiResponse<PricingRuleDto>
                .ErrorResponse(result.Error, GetErrorCode(result.StatusCode), result.StatusCode));

        return Ok(ApiResponse<PricingRuleDto>.SuccessResponse(result.Value, "Pricing rule found"));
    }

    [HttpPost]
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
    public async Task<ActionResult<ApiResponse<PricingRuleDto>>> Update(
        int id, UpdatePricingRuleDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id)
            return BadRequest("ID in URL does not match ID in body.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _pricingRuleService.UpdatePricingRuleAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }

    [HttpDelete("{id:int}")]
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