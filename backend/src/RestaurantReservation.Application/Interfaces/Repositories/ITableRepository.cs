using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Table>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, DateTime startTime, DateTime endTime, int capacity,
        CancellationToken ct = default);
    
    Task AddAsync(Table table, CancellationToken ct = default);
    Task UpdateAsync(Table table, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}