using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserDto>> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result<UserDto>> AuthenticateAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> DeactivateUserAsync(int id, CancellationToken ct = default);
}