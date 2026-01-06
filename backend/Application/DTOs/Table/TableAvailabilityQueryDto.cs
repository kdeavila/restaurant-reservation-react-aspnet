using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Table;

public record TableAvailabilityQueryDto
{
    [Required]
    public DateTime Date { get; init; }

    [Required]
    public TimeSpan StartTime { get; init; }

    [Required]
    public TimeSpan EndTime { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1")]
    public int NumberOfGuests { get; init; }
}
