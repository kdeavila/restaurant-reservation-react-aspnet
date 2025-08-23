using RestaurantReservation.Application.DTOs.Table;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITableService
{
    Task<TableDto?> CreateTableAsync(CreateTableDto dto, CancellationToken ct = default);
    Task<TableDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TableDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateTableAsync(UpdateTableDto dto, CancellationToken ct = default);
    Task<bool> DeleteTableAsync(int id, CancellationToken ct = default);
}