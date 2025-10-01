using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.TableType;

public class TableTypeQueryParams
{
    public string? Name { get; set; }
    public decimal? BasePrice { get; set; }

    // Pagination
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 5;
}