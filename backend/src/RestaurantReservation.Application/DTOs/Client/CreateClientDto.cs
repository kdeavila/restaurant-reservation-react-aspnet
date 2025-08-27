using System.ComponentModel.DataAnnotations;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Client;

public class CreateClientDto
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
}