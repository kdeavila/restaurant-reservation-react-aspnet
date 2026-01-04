using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Table;

public class CreateTableDto
{
    [Required]
    [Range(1, 50)]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(50)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public int TableTypeId { get; set; }
}
