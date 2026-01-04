using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Table;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface ITableService
{
    Task<(IEnumerable<TableDetailedDto> Data, PaginationMetadata pagination)> GetAllAsync(
        TableQueryParams queryParams,
        CancellationToken ct = default
    );

    Task<Result<TableDetailedDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<TableDetailedDto>> CreateAsync(CreateTableDto dto, CancellationToken ct = default);

    Task<Result> UpdateAsync(UpdateTableDto dto, CancellationToken ct = default);
    Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default);
}
