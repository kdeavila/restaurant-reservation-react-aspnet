using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.UseCases.PricingRules;

public class CreatePricingRuleUseCase(
    ITableTypeRepository tableTypeRepository,
    IPricingRuleService pricingRuleService,
    IPricingRuleDaysRepository pricingRuleDaysRepository
)
{
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;
    private readonly IPricingRuleService _pricingRuleService = pricingRuleService;
    private readonly IPricingRuleDaysRepository _pricingRuleDaysRepository = pricingRuleDaysRepository;

    public async Task<Result<PricingRuleDto>> ExecuteAsync(
        CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        var tableTypeExists = await _tableTypeRepository.GetByIdAsync(dto.TableTypeId, ct);
        if (tableTypeExists is null)
            return Result.Failure<PricingRuleDto>("TableTypeId does not exist.", 400);

        var pricingRule = await _pricingRuleService.CreatePricingRuleAsync(dto, ct);
        if (pricingRule.IsFailure)
            return Result.Failure<PricingRuleDto>(pricingRule.Error, pricingRule.StatusCode);

        var rule = pricingRule.Value;

        var pricingRuleDays = dto.DaysOfWeek.Select(day => new PricingRuleDays()
        {
            PricingRuleId = rule.Id,
            DayOfWeek = day
        }).ToList();

        foreach (var prd in pricingRuleDays)
            await _pricingRuleDaysRepository.AddAsync(prd, ct);

        var pricingRuleDto = new PricingRuleDto(
            rule.Id, rule.RuleName, rule.RuleType, rule.StartTime, rule.EndTime, rule.SurchargePercentage,
            rule.StartDate, rule.EndDate, rule.TableTypeId, rule.IsActive, dto.DaysOfWeek
        );
        return Result.Success(pricingRuleDto);
    }
}