namespace RestaurantReservation.Application.DTOs.User;

public record AuthDto(
    int Id,
    string Username,
    string Email, 
    string Role,
    string Status,
    string Token,
    DateTime TokenExpiry
);