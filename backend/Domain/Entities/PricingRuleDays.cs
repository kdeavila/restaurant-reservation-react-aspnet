using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class PricingRuleDays
{
    public int Id { get; set; }
    public int PricingRuleId { get; set; }
    public DaysOfWeek DayOfWeek { get; set; }

    // Navigation
    public PricingRule? PricingRule { get; set; }
}
