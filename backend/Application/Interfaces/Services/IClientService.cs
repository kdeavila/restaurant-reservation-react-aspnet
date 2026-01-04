using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Client;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IClientService
{
    Task<Result<ClientDto>> CreateAsync(CreateClientDto dto, CancellationToken ct = default);

    Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(IEnumerable<ClientDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        ClientQueryParams queryParams,
        CancellationToken ct = default
    );

    Task<Result> UpdateAsync(UpdateClientDto dto, CancellationToken ct = default);

    Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default);
}
