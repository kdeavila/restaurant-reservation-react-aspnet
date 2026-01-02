
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class Reservation
{
   public int Id { get; set; }
   public int ClientId { get; set; }
   public int TableId { get; set; }
   public DateTime Date { get; set; }
   public TimeSpan StartTime { get; set; }
   public TimeSpan EndTime { get; set; }
   public int NumberOfGuests { get; set; }
   public decimal BasePrice { get; set; }
   public decimal TotalPrice { get; set; }
   public ReservationStatus Status { get; set; }
   public string Notes { get; set; } = string.Empty;
   public DateTime CreatedAt { get; set; }
   public DateTime UpdatedAt { get; set; }
   public string CreatedByUserId { get; set; } = string.Empty;

   // Navigation
   public ApplicationUser User { get; set; } = null!;
   public Client Client { get; set; } = null!;
   public Table Table { get; set; } = null!;
}