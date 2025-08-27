using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Table;

public class UpdateTableDto
{
    [Required]
    public int Id { get; set; }
    [Range(1, 50)]
    public int? Capacity { get; set; }
    [MaxLength(50)]
    public string? Location { get; set; }
    public int? TableTypeId { get; set; }
    public string? Status { get; set; }
}