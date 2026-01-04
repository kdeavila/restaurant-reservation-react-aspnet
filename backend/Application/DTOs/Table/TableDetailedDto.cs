using RestaurantReservation.Application.DTOs.TableType;

namespace RestaurantReservation.Application.DTOs.Table;

public record TableDetailedDto(
    int Id,
    string Code,
    int Capacity,
    string Location,
    string Status,
    TableTypeSimpleDto TableType
);
