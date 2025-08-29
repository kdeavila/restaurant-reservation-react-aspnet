using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.TableType;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITableTypeService
{
    Task<Result<TableTypeDto>> CreateAsync(CreateTableTypeDto dto, CancellationToken ct = default);
    Task<Result<TableTypeDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TableTypeDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> UpdateAsync(UpdateTableTypeDto dto, CancellationToken ct = default);
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
}