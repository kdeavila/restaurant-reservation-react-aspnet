namespace RestaurantReservation.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; }
    public int? CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; }
    public int TableId { get; set; }
    public Table Table { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}