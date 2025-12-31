using Mapster;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class PricingRuleService(
    IPricingRuleRepository ruleRepo,
    IPricingRuleDaysRepository daysRepo,
    ITableTypeRepository tableTypeRepository
) : IPricingRuleService
{
   private readonly IPricingRuleRepository _pricingRuleRepository = ruleRepo;
   private readonly IPricingRuleDaysRepository _pricingRuleDaysRepository = daysRepo;
   private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

   public async Task<(IEnumerable<PricingRuleDto> Data, PaginationMetadata Pagination)> GetAllAsync(
       PricingRuleQueryParams queryParams, CancellationToken ct = default)
   {
      var query = _pricingRuleRepository.Query();

      if (!string.IsNullOrEmpty(queryParams.RuleName))
         query = query.Where(r => r.RuleName.Contains(queryParams.RuleName));

      if (!string.IsNullOrEmpty(queryParams.RuleType))
         query = query.Where(r => r.RuleType.Contains(queryParams.RuleType));

      if (queryParams.StartTime.HasValue)
         query = query.Where(r => r.StartTime >= queryParams.StartTime.Value);

      if (queryParams.EndTime.HasValue)
         query = query.Where(r => r.EndTime <= queryParams.EndTime.Value);

      if (queryParams.StartDate.HasValue)
         query = query.Where(r => r.StartDate >= queryParams.StartDate.Value);

      if (queryParams.EndDate.HasValue)
         query = query.Where(r => r.EndDate <= queryParams.EndDate.Value);

      if (queryParams.TableTypeId.HasValue)
         query = query.Where(r => r.TableTypeId == queryParams.TableTypeId.Value);

      if (queryParams.IsActive.HasValue)
         query = query.Where(r => r.IsActive == queryParams.IsActive.Value);

      // pagination
      var totalCount = await query.CountAsync(ct);
      var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;

      var rulesPage = await query
          .Include(r => r.PricingRuleDays)
          .Skip(skipNumber)
          .Take(queryParams.PageSize)
          .ToListAsync(ct);

      var data = rulesPage.Select(rule => rule.Adapt<PricingRuleDto>());

      var pagination = new PaginationMetadata
      {
         Page = queryParams.Page,
         PageSize = queryParams.PageSize,
         TotalCount = totalCount,
         TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize)
      };

      return (data, pagination);
   }

   public async Task<Result<PricingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default)
   {
      var rule = await _pricingRuleRepository.Query()
         .Include(r => r.PricingRuleDays)
         .FirstOrDefaultAsync(r => r.Id == id, ct);
         
      if (rule is null) return Result.Failure<PricingRuleDto>("Pricing rule not found.", 404);

      var pricingRuleDto = rule.Adapt<PricingRuleDto>();
      return Result.Success(pricingRuleDto);
   }

   public async Task<Result<PricingRule>> CreateAsync(
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

      if (dto.SurchargePercentage is < -100 or > 100)
         return Result.Failure<PricingRule>("SurchargePercentage must be between -100 and 100", 400);

      if (!dto.DaysOfWeek.Any())
         return Result.Failure<PricingRule>("At least one day of the week must be specified.", 400);

      // Validate DaysOfWeek enum values
      var validDaysOfWeek = Enum.GetValues<DaysOfWeek>();
      var invalidDays = dto.DaysOfWeek
          .Where(d => !validDaysOfWeek.Contains(d)).ToList();
      if (invalidDays.Any())
         return Result.Failure<PricingRule>($"Invalid days of week provided: " +
                                            $"{string.Join(", ", invalidDays)}", 400);

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

   public async Task<Result> UpdateAsync(UpdatePricingRuleDto dto, CancellationToken ct = default)
   {
      var rule = await _pricingRuleRepository.GetByIdAsync(dto.Id, ct);
      if (rule is null)
         return Result.Failure("Pricing rule not found.", 404);

      if (dto.StartTime is not null && dto.EndTime is not null)
      {
         var startTime = dto.StartTime.Value;
         var endTime = dto.EndTime.Value;

         if (startTime >= endTime)
            return Result.Failure("EndTime must be after StartTime", 400);
      }

      if (dto.StartDate is not null && dto.EndDate is not null)
      {
         var startDate = dto.StartDate.Value;
         var endDate = dto.EndDate.Value;

         if (startDate >= endDate)
            return Result.Failure("EndDate must be after StartDate", 400);
      }

      if (dto.SurchargePercentage is < -100 or > 100)
         return Result.Failure("SurchargePercentage must be between -100 and 100", 400);

      if (dto.DaysOfWeek is { Count: > 0 })
      {
         var invalidDays = dto.DaysOfWeek.Except(Enum.GetValues<DaysOfWeek>()).ToList();
         if (invalidDays.Any())
            return Result.Failure($"Invalid days of week provided: " +
                                  $"{string.Join(", ", invalidDays)}", 400);
      }
      else if (dto.DaysOfWeek is not null)
      {
         return Result.Failure("At least one day of the week must be specified.", 400);
      }

      if (dto.TableTypeId.HasValue && dto.TableTypeId.Value != rule.TableTypeId)
      {
         var tableType = await _tableTypeRepository.GetByIdAsync(dto.TableTypeId.Value, ct);
         if (tableType is null || !tableType.IsActive)
            return Result.Failure("Table type not found or inactive", 404);
      }

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
         await _pricingRuleDaysRepository.DeleteByPricingRuleIdAsync(rule.Id, ct);
         var dayEntities = dto.DaysOfWeek
             .Select(d => new PricingRuleDays
             {
                PricingRuleId = rule.Id,
                DayOfWeek = d
             }).ToList();
         await _pricingRuleDaysRepository.AddRangeAsync(dayEntities, ct);
      }

      return Result.Success();
   }

   public async Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default)
   {
      var rule = await _pricingRuleRepository.GetByIdAsync(id, ct);
      if (rule is null)
         return Result.Failure<string>("Pricing rule not found.", 404);

      rule.IsActive = false;
      await _pricingRuleRepository.UpdateAsync(rule, ct);

      return Result.Success<string>("Pricing deactivated successfully");
   }
}