using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}
