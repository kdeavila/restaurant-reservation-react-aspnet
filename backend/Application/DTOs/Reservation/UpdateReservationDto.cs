using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class UpdateReservationDto
{
    [Required]
    public int Id { get; set; }
    public int? TableId { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    [Range(1,50)]
    public int? NumberOfGuests { get; set; }
    [MaxLength(200)]
    public string? Notes { get; set; }
    public string? Status { get; set; }
}