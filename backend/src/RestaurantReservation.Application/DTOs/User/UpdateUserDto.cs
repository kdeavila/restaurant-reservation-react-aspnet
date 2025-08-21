using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.User;

public class UpdateUserDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public UserRole? UserRole { get; set; }
}