namespace RestaurantReservation.Application.DTOs.TableType;

public record TableTypeDto(
    int Id,
    string Name,
    string Description,
    decimal BasePricePerHour,
    bool IsActive,
    int TableCount,
    DateTime CreatedAt
);