namespace RestaurantReservation.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? Role { get; }
}