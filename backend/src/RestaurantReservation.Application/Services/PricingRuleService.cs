using Microsoft.VisualBasic.CompilerServices;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class PricingRuleService(
    IPricingRuleRepository ruleRepo,
    IPricingRuleDaysRepository daysRepo,
    ITableTypeRepository _tableTypeRepository
) : IPricingRuleService
{
    private readonly IPricingRuleRepository _pricingRuleRepository = ruleRepo;
    private readonly IPricingRuleDaysRepository _pricingRuleDaysRepository = daysRepo;
    private readonly ITableTypeRepository _tableTypeRepository = _tableTypeRepository;

    public async Task<Result<PricingRule>> CreatePricingRuleAsync(
        CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(dto.RuleName))
            return Result.Failure<PricingRule>("RuleName cannot be empty.", 400);

        if (string.IsNullOrEmpty(dto.RuleType))
            return Result.Failure<PricingRule>("RuleType cannot be empty.", 400);

        if (dto.StartTime >= dto.EndTime)
            return Result.Failure<PricingRule>("EndTime must be after StartTime.", 400);

        if (dto.StartDate >= dto.EndDate)
            return Result.Failure<PricingRule>("EndDate must be after StartDate", 400);

        if (dto.SurchargePercentage is < 0 or > 100)
            return Result.Failure<PricingRule>("SurchargePercentage must be between 0 and 100", 400);

        if (!dto.DaysOfWeek.Any())
            return Result.Failure<PricingRule>("At least one day of the week must be specified.", 400);

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
        return Result.Success(pricingRule);
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

        if (dto.TableTypeId.HasValue)
        {
            var tableTypeExists = await _tableTypeRepository.GetByIdAsync(dto.TableTypeId.Value, ct);

            if (tableTypeExists is null)
                return Result.Failure("Table type not found.", 404);
        }

        if (dto.RuleName != null && string.IsNullOrEmpty(dto.RuleName))
            return Result.Failure("RuleName cannot be empty.", 400);

        if (dto.RuleType != null && string.IsNullOrEmpty(dto.RuleType))
            return Result.Failure("RuleType cannot be empty.", 400);

        if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime.Value >= dto.EndTime.Value)
            return Result.Failure("EndTime must be after StartTime.", 400);

        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate.Value >= dto.EndDate.Value)
            return Result.Failure("EndDate must be after StartDate", 400);

        if (dto.SurchargePercentage.HasValue && (dto.SurchargePercentage < 0 || dto.SurchargePercentage > 100))
            return Result.Failure("SurchargePercentage must be between 0 and 100", 400);

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

        if (dto.DaysOfWeek != null)
        {
            if (!dto.DaysOfWeek.Any())
                return Result.Failure("At least one day of the week must be specified.", 400);

            await _pricingRuleDaysRepository.DeleteByPricingRuleIdAsync(rule.Id, ct);

            var dayEntities = dto.DaysOfWeek.Select(d => new PricingRuleDays()
            {
                PricingRuleId = rule.Id,
                DayOfWeek = d
            }).ToList();

            await _pricingRuleDaysRepository.AddRangeAsync(dayEntities, ct);
        }
        
        await _pricingRuleRepository.UpdateAsync(rule, ct);
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