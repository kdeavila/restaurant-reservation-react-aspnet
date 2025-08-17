using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Reservation> ReservationsCreated { get; set; } = new List<Reservation>();
}