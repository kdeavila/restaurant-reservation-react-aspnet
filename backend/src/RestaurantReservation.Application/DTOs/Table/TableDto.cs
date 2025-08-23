namespace RestaurantReservation.Application.DTOs.Table;

public record TableDto(
    int Id,
    string Code,
    int Capacity,
    string Location,
    int TableTypeId,
    string Status
);