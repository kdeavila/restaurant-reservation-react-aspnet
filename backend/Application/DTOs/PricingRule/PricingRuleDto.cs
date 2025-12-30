using RestaurantReservation.Application.DTOs.TableType;
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
    TableTypeSimpleDto TableType,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<DaysOfWeek> DaysOfWeek
);