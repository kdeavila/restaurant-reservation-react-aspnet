using RestaurantReservation.Application.DTOs.TableType;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITableTypeService
{
    Task<TableTypeDto?> CreateAsync(CreateTableTypeDto dto, CancellationToken ct = default);
    Task<TableTypeDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TableTypeDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateAsync(UpdateTableTypeDto dto, CancellationToken ct = default);
    Task<bool> DeactivateAsync(int id, CancellationToken ct = default);
}