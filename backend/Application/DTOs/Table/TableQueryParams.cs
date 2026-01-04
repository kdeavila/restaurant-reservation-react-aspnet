using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Application.DTOs.Table;

public class TableQueryParams
{
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }

    // Pagination
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
}
