namespace RestaurantReservation.Application.DTOs.Table;

public class CreateTableDto
{
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
    public int TableTypeId { get; set; }
}