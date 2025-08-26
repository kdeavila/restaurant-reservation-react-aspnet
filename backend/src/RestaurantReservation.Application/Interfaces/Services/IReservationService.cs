using RestaurantReservation.Application.DTOs.Reservation;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationDto?> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default);
    Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId, CancellationToken ct = default);
    Task<bool> UpdateReservationAsync(UpdateReservationDto dto, CancellationToken ct = default);
    Task<bool> CancelReservationAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteReservationAsync(int id, CancellationToken ct = default);
}