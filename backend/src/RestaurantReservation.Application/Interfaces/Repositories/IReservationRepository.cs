using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetByTableIdAsync(int tableId, CancellationToken ct = default);

    Task<bool> ExistsOverlappingReservationAsync(int tableId, DateTime date, TimeSpan startTime, TimeSpan endTime,
        CancellationToken ct = default);

    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}