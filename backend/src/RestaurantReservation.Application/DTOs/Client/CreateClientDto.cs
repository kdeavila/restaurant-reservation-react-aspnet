using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Client;

public class CreateClientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}