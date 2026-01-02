using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Services;

public class TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager) : ITokenService
{
   private readonly string _key = configuration["Jwt:Key"]!;
   private readonly string _issuer = configuration["Jwt:Issuer"]!;
   private readonly string _audience = configuration["Jwt:Audience"]!;
   private readonly int _expiryInMinutes = configuration.GetValue<int>("Jwt:ExpiryInMinutes");
   private readonly UserManager<ApplicationUser> _userManager = userManager;

   public string GenerateToken(ApplicationUser user)
   {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_key);

      var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

      var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? $"Employee {user.Id}"),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      var tokenDescriptor = new SecurityTokenDescriptor()
      {
         Subject = new ClaimsIdentity(claims),
         Expires = DateTime.UtcNow.AddMinutes(_expiryInMinutes),
         Issuer = _issuer,
         Audience = _audience,
         SigningCredentials = new SigningCredentials(
              new SymmetricSecurityKey(key),
              SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
   }
}