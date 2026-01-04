using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService
) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Result<UserDto>> RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct = default
    )
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Result.Failure<UserDto>("Email address is already in use.", 409);

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            Status = ApplicationUserStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure<UserDto>(errors, 400);
        }

        var roleName = dto.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        var userDto = new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roleName,
            user.Status.ToString()
        );

        return Result.Success(userDto);
    }

    public async Task<Result<AuthDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Result.Failure<AuthDto>("Invalid credentials.", 401);

        if (user.Status != ApplicationUserStatus.Active)
            return Result.Failure<AuthDto>("User account inactive.", 403);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!signInResult.Succeeded)
            return Result.Failure<AuthDto>("Invalid credentials.", 401);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        var token = _tokenService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiryUtc = jwtToken.ValidTo;

        var authResponse = new AuthDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            primaryRole,
            user.Status.ToString(),
            token,
            expiryUtc
        );

        return Result.Success(authResponse);
    }

    public async Task<Result<UserDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Result.Failure<UserDto>("User not found.", 404);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var userDto = new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            role,
            user.Status.ToString()
        );
        return Result.Success(userDto);
    }

    public async Task<(IEnumerable<UserDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        UserQueryParams queryParams,
        CancellationToken ct = default
    )
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(queryParams.Username))
            query = query.Where(u => u.UserName!.Contains(queryParams.Username));

        if (!string.IsNullOrEmpty(queryParams.Email))
            query = query.Where(u => u.Email!.Contains(queryParams.Email));

        if (
            !string.IsNullOrEmpty(queryParams.Status)
            && Enum.TryParse<ApplicationUserStatus>(queryParams.Status, true, out var status)
        )
            query = query.Where(u => u.Status == status);

        if (!string.IsNullOrEmpty(queryParams.Role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(queryParams.Role);
            var ids = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => ids.Contains(u.Id));
        }

        var totalCount = await query.CountAsync(ct);
        var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;

        var usersPage = await query.Skip(skipNumber).Take(queryParams.PageSize).ToListAsync(ct);

        var data = new List<UserDto>();
        foreach (var user in usersPage)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;
            data.Add(
                new UserDto(
                    user.Id,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty,
                    role,
                    user.Status.ToString()
                )
            );
        }

        var pagination = new PaginationMetadata
        {
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize),
        };

        return (data, pagination);
    }

    public async Task<Result<string>> DeactivateAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result.Failure<string>("User not found.", 404);

        if (user.Status == ApplicationUserStatus.Inactive)
            return Result.Success("User is already inactive.");

        user.Status = ApplicationUserStatus.Inactive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure<string>(errors, 400);
        }

        return Result.Success("User deactivated successfully");
    }
}
