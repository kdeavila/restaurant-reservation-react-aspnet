using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<AuthResponse>> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> DeactivateUserAsync(int id, CancellationToken ct = default);
}