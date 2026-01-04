namespace RestaurantReservation.Application.DTOs.Client;

public record ClientDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    int TotalReservations,
    DateTime CreatedAt
);
