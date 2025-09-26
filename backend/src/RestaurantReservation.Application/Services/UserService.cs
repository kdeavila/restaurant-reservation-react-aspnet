using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class UserService(IUserRepository userRepository, ITokenService tokenService) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Result<UserDto>> RegisterUserAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        if (await _userRepository.GetByEmailAsync(dto.Email, ct) is not null)
            return Result.Failure<UserDto>("Email address is already in use.", 409);

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

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.Status.ToString()
        );
        return Result.Success(userDto);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>("Invalid credentials.", 401);

        if (user.Status != UserStatus.Active)
            return Result.Failure<AuthResponse>("User account inactive.", 403);

        var token = _tokenService.GenerateToken(user);

        var authResponse = new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.Status.ToString(),
            token
        );

        return Result.Success<AuthResponse>(authResponse);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return Result.Failure<UserDto>("User not found.", 404);

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.Status.ToString()
        );
        return Result.Success(userDto);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.Role.ToString(),
                u.Status.ToString()
            )
        );
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