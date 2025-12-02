using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;

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

    public async Task<Result<AuthDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result.Failure<AuthDto>("Invalid credentials.", 401);

        if (user.Status != UserStatus.Active)
            return Result.Failure<AuthDto>("User account inactive.", 403);

        var token = _tokenService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiryUtc = jwtToken.ValidTo;

        var authResponse = new AuthDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.Status.ToString(),
            token,
            expiryUtc
        );

        return Result.Success<AuthDto>(authResponse);
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

    public async Task<(IEnumerable<UserDto> Data, PaginationMetadata Pagination)> GetAllAsync(UserQueryParams queryParams, CancellationToken ct = default)
    {
        var query = _userRepository.Query().AsNoTracking();

        if (!string.IsNullOrEmpty(queryParams.Username))
            query = query.Where(u => u.Username.Contains(queryParams.Username));
        
        if (!string.IsNullOrEmpty(queryParams.Email))
            query = query.Where(u => u.Email.Contains(queryParams.Email));

        if (!string.IsNullOrEmpty(queryParams.Role) && Enum.TryParse<UserRole>(queryParams.Role, true, out var role))
            query = query.Where(u => u.Role == role);

        if (!string.IsNullOrEmpty(queryParams.Status) && Enum.TryParse<UserStatus>(queryParams.Status, true, out var status))
            query = query.Where(u => u.Status == status);

        var totalCount = await query.CountAsync(ct);
        var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;

        var usersPage = await query
            .Skip(skipNumber)
            .Take(queryParams.PageSize)
            .ToListAsync(ct);

        var data = usersPage.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.Email,
            u.Role.ToString(),
            u.Status.ToString()
        ));

        var pagination = new PaginationMetadata
        {
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize)
        };

        return (data, pagination);
    }

    public async Task<Result<string>> DeactivateUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return Result.Failure<string>("User not found.", 404);

        if (user.Status == UserStatus.Inactive)
        {
            return Result.Failure<string>("User already deactivated.", 422);
        }

        user.Status = UserStatus.Inactive;
        await _userRepository.UpdateAsync(user, ct);
        return Result.Success("User deactivated successfully");
    }
}