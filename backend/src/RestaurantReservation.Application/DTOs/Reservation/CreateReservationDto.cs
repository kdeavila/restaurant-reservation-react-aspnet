namespace RestaurantReservation.Application.DTOs.Reservation;

public class CreateReservationDto
{
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    public string? Notes { get; set; }

}