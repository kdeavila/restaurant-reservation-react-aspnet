using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableTypeRepository
{
    Task<TableType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TableType>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TableType tableType, CancellationToken ct = default);
    Task UpdateAsync(TableType tableType, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}