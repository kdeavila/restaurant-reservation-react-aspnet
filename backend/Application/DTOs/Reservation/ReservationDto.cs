using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.DTOs.Reservation;

public record ReservationDto(
    int Id,
    ClientDto Client,
    TableDto Table,
    DateOnly Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int NumberOfGuests,
    decimal BasePrice,
    decimal TotalPrice,
    string Status,
    string Notes,
    UserSimpleDto User
);
