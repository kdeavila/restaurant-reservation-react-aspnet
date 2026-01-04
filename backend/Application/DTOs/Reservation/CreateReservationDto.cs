using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Reservation;

public class CreateReservationDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int TableId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Range(1, 50)]
    public int NumberOfGuests { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }
}
