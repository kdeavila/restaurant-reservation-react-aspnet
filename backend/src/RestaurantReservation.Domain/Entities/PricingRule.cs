namespace RestaurantReservation.Domain.Entities;

public class PricingRule
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal SurchargePercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TableTypeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public TableType TableType { get; set; } = null!;
    public ICollection<PricingRuleDays> PricingRuleDays { get; set; } = new List<PricingRuleDays>();
}