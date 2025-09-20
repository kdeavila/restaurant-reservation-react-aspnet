namespace RestaurantReservation.Application.DTOs.Reservation;

public class ReservationQueryParams
{
    public int? ClientId { get; set; }
    public int? TableId { get; set; }
    public string? Status { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;

    // TODO: add the logic for order and sort
    public string? OrderBy { get; set; }
    public string? SortBy { get; set; }
}