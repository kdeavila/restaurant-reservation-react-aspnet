using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableRepository
{
    IQueryable<Table> Query();
    Task<Table?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Table>> GetByTableTypeIdAsync(int tableTypeId, CancellationToken ct = default);

    Task<Dictionary<int, int>> GetTableCountsByTableTypeIdsAsync(
        IEnumerable<int> tableTypeIds,
        CancellationToken ct = default
    );

    Task<IEnumerable<Table>> GetAvailableTablesAsync(
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        int capacity,
        CancellationToken ct = default
    );

    Task AddAsync(Table table, CancellationToken ct = default);
    Task UpdateAsync(Table table, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
