using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserDto?> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null) return null;

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
        return new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
    }

    public async Task<UserDto?> AuthenticateAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null) return null;

        var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValid) return null;

        return new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return null;

        return new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.Status.ToString());
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role.ToString(), u.Status.ToString()));
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id, ct);
        if (user is null) return false;

        user.Username = dto.Username ?? user.Username;
        user.Email = dto.Email ?? user.Email;
        user.Role = dto.Role ?? user.Role;

        await _userRepository.UpdateAsync(user, ct);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return false;

        user.Status = UserStatus.Inactive;
        await _userRepository.UpdateAsync(user, ct);
        return true;
    }
}