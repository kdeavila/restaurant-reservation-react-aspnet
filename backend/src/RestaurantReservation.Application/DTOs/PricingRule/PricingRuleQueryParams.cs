using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.PricingRule;

public class PricingRuleQueryParams
{
    public string? RuleName { get; set; }
    public string? RuleType { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TableTypeId { get; set; }
    public bool? IsActive { get; set; }

    // Pagination
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 5;
}