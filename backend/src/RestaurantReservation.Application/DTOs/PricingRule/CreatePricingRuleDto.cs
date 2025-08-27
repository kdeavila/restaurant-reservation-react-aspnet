using System.ComponentModel.DataAnnotations;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public class CreatePricingRuleDto
{
    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string RuleType { get; set; } = string.Empty;
    [Required]
    public TimeSpan StartTime { get; set; }
    [Required]
    public TimeSpan EndTime { get; set; }
    [Required]
    [Range(0, 100)]
    public decimal SurchargePercentage { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public int TableTypeId { get; set; }
    [Required]
    public List<DaysOfWeek> DaysOfWeek { get; set; } = new List<DaysOfWeek>();
}