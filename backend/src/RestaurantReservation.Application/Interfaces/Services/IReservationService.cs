using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Result<Reservation>> CreateReservationAsync(CreateReservationDto dto, decimal basePrice, decimal totalPrice,
        CancellationToken ct = default);

    Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId, CancellationToken ct = default);
    Task<Result> UpdateReservationAsync(Reservation reservation, CancellationToken ct = default);
    Task<Result> CancelReservationAsync(int id, CancellationToken ct = default);
    Task<Result> DeleteReservationAsync(int id, CancellationToken ct = default);
}