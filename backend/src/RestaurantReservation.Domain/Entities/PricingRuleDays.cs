namespace RestaurantReservation.Domain.Entities;

public class PricingRuleDays
{
    public int Id { get; set; }
    public int PricingRuleId { get; set; }
    public PricingRule PricingRule { get; set; }

    public int DayOfWeek { get; set; }
}