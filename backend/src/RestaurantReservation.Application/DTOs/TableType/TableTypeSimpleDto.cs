namespace RestaurantReservation.Application.DTOs.TableType;

public record TableTypeSimpleDto(
    int Id,
    string Name,
    decimal BasePricePerHour,
    bool IsActive
);