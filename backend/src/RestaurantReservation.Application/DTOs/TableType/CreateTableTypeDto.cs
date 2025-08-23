namespace RestaurantReservation.Application.DTOs.TableType;

public class CreateTableTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePricePerHour { get; set; }
}