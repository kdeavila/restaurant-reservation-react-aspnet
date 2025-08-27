namespace RestaurantReservation.Application.DTOs.Reservation;

public class CreateReservationDto
{
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    public string? Notes { get; set; }
}