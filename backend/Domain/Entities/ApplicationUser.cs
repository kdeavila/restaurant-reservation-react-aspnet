using Microsoft.AspNetCore.Identity;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public ApplicationUserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Reservation> ReservationsCreated { get; set; } = new List<Reservation>();
}
