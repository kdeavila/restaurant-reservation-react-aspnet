using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Services;

public class PricingRuleService(
    IPricingRuleRepository ruleRepo,
    IPricingRuleDaysRepository daysRepo
) : IPricingRuleService
{
    private readonly IPricingRuleRepository _pricingRuleRepository = ruleRepo;
    private readonly IPricingRuleDaysRepository _pricingRuleDaysRepository = daysRepo;

    public async Task<Result<PricingRuleDto>> CreatePricingRuleAsync(
        CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        if (!dto.DaysOfWeek.Any())
            return Result.Failure<PricingRuleDto>("At least one day of the week must be specified.", 400);

        var pricingRule = new PricingRule()
        {
            RuleName = dto.RuleName,
            RuleType = dto.RuleType,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            SurchargePercentage = dto.SurchargePercentage,
            TableTypeId = dto.TableTypeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _pricingRuleRepository.AddAsync(pricingRule, ct);

        var pricingRuleDays = dto.DaysOfWeek.Select(day => new PricingRuleDays()
        {
            PricingRuleId = pricingRule.Id,
            DayOfWeek = day
        }).ToList();

        foreach (var prd in pricingRuleDays)
            await _pricingRuleDaysRepository.AddAsync(prd, ct);

        var pricingRuleDto = new PricingRuleDto(
            pricingRule.Id, pricingRule.RuleName, pricingRule.RuleType, pricingRule.StartTime, pricingRule.EndTime,
            pricingRule.SurchargePercentage, pricingRule.StartDate, pricingRule.EndDate, pricingRule.TableTypeId,
            pricingRule.IsActive, dto.DaysOfWeek
        );
        return Result.Success(pricingRuleDto);
    }

    public async Task<Result<PricingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rule = await _pricingRuleRepository.GetByIdAsync(id, ct);
        if (rule is null) return Result.Failure<PricingRuleDto>("Pricing rule not found.", 404);

        var days = await _pricingRuleDaysRepository.GetByPricingRuleIdAsync(rule.Id, ct);

        var pricingRuleDto = new PricingRuleDto(
            rule.Id, rule.RuleName, rule.RuleType, rule.StartTime, rule.EndTime,
            rule.SurchargePercentage, rule.StartDate, rule.EndDate, rule.TableTypeId,
            rule.IsActive, days.Select(d => d.DayOfWeek).ToList()
        );
        return Result.Success(pricingRuleDto);
    }

    public async Task<IEnumerable<PricingRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rules = await _pricingRuleRepository.GetAllAsync(ct);
        var result = new List<PricingRuleDto>();

        foreach (var rule in rules)
        {
            var days = await _pricingRuleDaysRepository.GetByPricingRuleIdAsync(rule.Id, ct);

            result.Add(new PricingRuleDto(
                rule.Id, rule.RuleName, rule.RuleType, rule.StartTime, rule.EndTime,
                rule.SurchargePercentage, rule.StartDate, rule.EndDate, rule.TableTypeId,
                rule.IsActive, days.Select(d => d.DayOfWeek).ToList()
            ));
        }

        return result;
    }

    public async Task<Result> UpdatePricingRuleAsync(UpdatePricingRuleDto dto, CancellationToken ct = default)
    {
        var rule = await _pricingRuleRepository.GetByIdAsync(dto.Id, ct);
        if (rule is null) return Result.Failure("Pricing rule not found.", 404);

        rule.RuleName = dto.RuleName ?? rule.RuleName;
        rule.RuleType = dto.RuleType ?? rule.RuleType;
        rule.StartTime = dto.StartTime ?? rule.StartTime;
        rule.EndTime = dto.EndTime ?? rule.EndTime;
        rule.SurchargePercentage = dto.SurchargePercentage ?? rule.SurchargePercentage;
        rule.StartDate = dto.StartDate ?? rule.StartDate;
        rule.EndDate = dto.EndDate ?? rule.EndDate;
        rule.TableTypeId = dto.TableTypeId ?? rule.TableTypeId;
        rule.IsActive = dto.IsActive ?? rule.IsActive;

        await _pricingRuleRepository.UpdateAsync(rule, ct);

        if (dto.DaysOfWeek is null || !dto.DaysOfWeek.Any()) return Result.Success();
        await _pricingRuleDaysRepository.DeleteByPricingRuleIdAsync(rule.Id, ct);

        var dayEntities = dto.DaysOfWeek.Select(day => new PricingRuleDays
        {
            PricingRuleId = rule.Id,
            DayOfWeek = day
        }).ToList();

        await _pricingRuleDaysRepository.AddRangeAsync(dayEntities, ct);
        return Result.Success();
    }

    public async Task<Result> DeletePricingRuleAsync(int id, CancellationToken ct = default)
    {
        var rule = await _pricingRuleRepository.GetByIdAsync(id, ct);
        if (rule is null) return Result.Failure("Pricing rule not found.", 404);

        await _pricingRuleDaysRepository.DeleteByPricingRuleIdAsync(rule.Id, ct);
        await _pricingRuleRepository.DeleteAsync(id, ct);

        return Result.Success();
    }
}