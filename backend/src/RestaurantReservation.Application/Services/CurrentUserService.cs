using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return int.TryParse(userIdClaim, out var userId) ? userId : (int?)null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.Name)?.Value;
    public string? Email => _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.Email)?.Value;
    public string? Role => _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.Role)?.Value;
}