using System.ComponentModel.DataAnnotations;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.User;

public class UpdateUserDto
{
    [Required]
    public int Id { get; set; }
    [MaxLength(50)]
    public string? Username { get; set; }
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
}