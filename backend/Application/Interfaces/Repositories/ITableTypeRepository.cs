using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableTypeRepository
{
    IQueryable<TableType> Query();
    Task<TableType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(TableType tableType, CancellationToken ct = default);
    Task UpdateAsync(TableType tableType, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
