namespace RestaurantReservation.Domain.Entities;

public class TableType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePricePerHour { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Table> Tables { get; set; } = new List<Table>();
    public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
}