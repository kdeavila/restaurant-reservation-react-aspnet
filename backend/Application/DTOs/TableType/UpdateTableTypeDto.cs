using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.TableType;

public class UpdateTableTypeDto
{
    [Required]
    public int Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [Range(1, 100)]
    public decimal? BasePricePerHour { get; set; }
}