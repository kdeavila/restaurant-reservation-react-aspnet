using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public class UpdatePricingRuleDto
{
    public int Id { get; set; }
    public string? RuleName { get; set; }
    public string? RuleType { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? SurchargePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TableTypeId { get; set; }
    public bool? IsActive { get; set; }
    public List<DaysOfWeek>? DaysOfWeek { get; set; }
}