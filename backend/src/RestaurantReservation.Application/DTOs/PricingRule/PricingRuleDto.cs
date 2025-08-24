using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public record PricingRuleDto(
    int Id,
    string RuleName,
    string RuleType,
    DateTime StartTime,
    DateTime EndTime,
    decimal SurchargePercentage,
    DateTime StartDate,
    DateTime EndDate,
    int TableTypeId,
    bool IsActive,
    IEnumerable<DaysOfWeek> DaysOfWeek
);