namespace RestaurantReservation.Application.DTOs.TableType;

public class UpdateTableTypeDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePricePerHour { get; set; }
}