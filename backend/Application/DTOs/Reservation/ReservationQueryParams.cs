namespace RestaurantReservation.Application.DTOs.Reservation;
using System.ComponentModel.DataAnnotations;

public class ReservationQueryParams
{
    public int? ClientId { get; set; }
    public int? TableId { get; set; }
    public string? Status { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Pagination
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 5;
}