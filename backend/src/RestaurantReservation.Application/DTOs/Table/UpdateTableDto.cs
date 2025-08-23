namespace RestaurantReservation.Application.DTOs.Table;

public class UpdateTableDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public string? Location { get; set; }
    public int? TableTypeId { get; set; }
    public string? Status { get; set; }
}