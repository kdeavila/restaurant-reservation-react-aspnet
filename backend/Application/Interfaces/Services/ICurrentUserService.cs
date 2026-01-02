namespace RestaurantReservation.Application.Interfaces.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? Role { get; }
}