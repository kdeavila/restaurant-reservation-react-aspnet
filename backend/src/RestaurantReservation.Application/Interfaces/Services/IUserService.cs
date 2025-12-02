using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserDto>> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result<AuthDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IEnumerable<UserDto> Data, PaginationMetadata Pagination)> GetAllAsync(UserQueryParams queryParams, CancellationToken ct = default);
    Task<Result<string>> DeactivateUserAsync(int id, CancellationToken ct = default);
}