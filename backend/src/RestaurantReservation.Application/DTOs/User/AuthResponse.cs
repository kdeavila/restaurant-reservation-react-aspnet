namespace RestaurantReservation.Application.DTOs.User;

public record AuthResponse(
    int Id,
    string Username,
    string Email, 
    string Role,
    string Status,
    string Token
);