namespace RestaurantReservation.Application.DTOs.TableType;

public class TableTypeQueryParams
{
    public string? Name { get; set; }
    public decimal? BasePrice { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}