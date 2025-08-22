using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Client;

public class UpdateClientDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; }
}