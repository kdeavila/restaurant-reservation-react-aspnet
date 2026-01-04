using System.ComponentModel.DataAnnotations;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public class UpdatePricingRuleDto
{
    [Required]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? RuleName { get; set; }

    [MaxLength(50)]
    public string? RuleType { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    [Range(-100, 100)]
    public decimal? SurchargePercentage { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? TableTypeId { get; set; }
    public bool? IsActive { get; set; }
    public List<DaysOfWeek>? DaysOfWeek { get; set; }
}
