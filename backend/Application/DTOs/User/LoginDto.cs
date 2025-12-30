using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.User;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}