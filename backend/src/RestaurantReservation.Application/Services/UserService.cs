using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<UserDto>> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null) return Result.Failure<UserDto>("A user with this email already exists.", 409);

        var user = new User()
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);
        var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
        return Result.Success(userDto);
    }

    public async Task<Result<UserDto>> AuthenticateAsync(LoginDto dto, CancellationToken ct = default)
    {
        // TODO: Implement JWT token generation and return token along with user info.
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null) return Result.Failure<UserDto>("Invalid email or password.", 401);

        var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValid) return Result.Failure<UserDto>("Invalid email or password.", 401);

        var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
        return Result.Success(userDto);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return Result.Failure<UserDto>("User not found.", 404);

        var userDto = new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
        return Result.Success(userDto);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role.ToString(), u.Status.ToString()));
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id, ct);
        if (user is null) return Result.Failure("User not found.", 404);

        user.Username = dto.Username ?? user.Username;
        user.Email = dto.Email ?? user.Email;
        user.Role = dto.Role ?? user.Role;

        await _userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return Result.Failure("User not found.", 404);

        user.Status = UserStatus.Inactive;
        await _userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }
}