using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public record PricingRuleDto(
    int Id,
    string RuleName,
    string RuleType,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal SurchargePercentage,
    DateTime StartDate,
    DateTime EndDate,
    int TableTypeId,
    bool IsActive,
    IEnumerable<DaysOfWeek> DaysOfWeek
);