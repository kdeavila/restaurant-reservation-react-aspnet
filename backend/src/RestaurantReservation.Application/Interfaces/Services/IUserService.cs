using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto?> AuthenticateAsync(LoginDto dto, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<bool> DeactivateUserAsync(int id, CancellationToken ct = default);
}