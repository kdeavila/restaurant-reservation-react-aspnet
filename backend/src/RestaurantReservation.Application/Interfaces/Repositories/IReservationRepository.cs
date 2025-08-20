using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);
    Task<IReadOnlyList<Reservation>> ListByDateAsync(DateTime date);
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task CancelAsync(int id, int cancelledByUserId);
    Task<bool> HasOverlapAsync(int tableId, DateTime date, TimeSpan start, TimeSpan end, int? ignoreReservationId = null);
}