using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public class CreatePricingRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal SurchargePercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TableTypeId { get; set; }
    public List<DaysOfWeek> DaysOfWeek { get; set; } = new List<DaysOfWeek>();
}