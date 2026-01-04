namespace RestaurantReservation.Domain.Entities;

public class PricingRule
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal SurchargePercentage { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TableTypeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public TableType TableType { get; set; } = null!;
    public ICollection<PricingRuleDays> PricingRuleDays { get; set; } = new List<PricingRuleDays>();
}
