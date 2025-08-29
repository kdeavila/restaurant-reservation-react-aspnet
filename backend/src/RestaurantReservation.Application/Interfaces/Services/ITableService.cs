using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Table;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITableService
{
    Task<Result<TableDto>> CreateTableAsync(CreateTableDto dto, CancellationToken ct = default);
    Task<Result<TableDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TableDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> UpdateTableAsync(UpdateTableDto dto, CancellationToken ct = default);
    Task<Result> DeleteTableAsync(int id, CancellationToken ct = default);
}