using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Table;

public class UpdateTableDto
{
    [Required] public int Id { get; set; }
    [Range(1, 50)] public int? Capacity { get; set; }
    [MaxLength(50)] public string? Location { get; set; }
    // Table type cannot be changed after creation. This field is ignored if provided.
    public int? TableTypeId { get; set; }
    public string? Status { get; set; }
}