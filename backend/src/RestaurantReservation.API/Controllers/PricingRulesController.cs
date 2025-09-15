using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _pricingRuleService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _pricingRuleService.GetByIdAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _createPricingRuleUseCase.ExecuteAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(
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
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _pricingRuleService.DeletePricingRuleAsync(id, ct);
        return result.IsFailure 
            ? StatusCode(result.StatusCode, new { error = result.Error }) 
            : NoContent();
    }
}