using System.ComponentModel.DataAnnotations;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Client;

public class UpdateClientDto
{
    [Required]
    public int Id { get; set; }
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }
    public string? Status { get; set; }
}