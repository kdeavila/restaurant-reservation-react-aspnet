using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Result<ReservationDto>> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default);
    Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId, CancellationToken ct = default);
    Task<Result> UpdateReservationAsync(UpdateReservationDto dto, CancellationToken ct = default);
    Task<Result> CancelReservationAsync(int id, CancellationToken ct = default);
    Task<Result> DeleteReservationAsync(int id, CancellationToken ct = default);
}