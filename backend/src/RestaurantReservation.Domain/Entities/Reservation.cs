using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Status Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    
    public Client Client { get; set; } = null!;
    public Table Table { get; set; } = null!;
}