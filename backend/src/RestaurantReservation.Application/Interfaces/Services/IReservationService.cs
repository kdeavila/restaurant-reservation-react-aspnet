using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetAllAsync(CancellationToken ct = default);

    Task<Result<Reservation>> CreateReservationAsync(CreateReservationDto dto, int createdByUserId, decimal basePrice,
        decimal totalPrice,
        CancellationToken ct = default);

    Task<Result> UpdateReservationAsync
        (UpdateReservationDto dto, decimal? basePrice, decimal? totalPrice, CancellationToken ct = default);

    Task<Result> CancelReservationAsync(int id, CancellationToken ct = default);
}