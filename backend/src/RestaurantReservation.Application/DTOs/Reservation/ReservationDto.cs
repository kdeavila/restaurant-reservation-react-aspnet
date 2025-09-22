using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.DTOs.Reservation;

public record ReservationDto(
    int Id,
    ClientDto Client,
    TableDto Table,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int NumberOfGuests,
    decimal BasePrice,
    decimal TotalPrice,
    string Status,
    string Notes,
    UserDto User
);