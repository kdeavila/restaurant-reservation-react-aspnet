using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Reservation;

public record ReservationDto(
    int Id,
    int ClientId,
    string ClientName,
    int TableId,
    string TableCode,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int NumberOfGuests,
    decimal BasePrice,
    decimal TotalPrice,
    string Status,
    string Notes
);