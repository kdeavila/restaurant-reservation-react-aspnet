using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Client;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IClientService
{
    Task<Result<ClientDto>> CreateClientAsync(
        CreateClientDto dto, CancellationToken ct = default);

    Task<Result<ClientDto>> GetByIdAsync(
        int id, CancellationToken ct = default);

    Task<(IEnumerable<ClientDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        ClientQueryParams queryParams, CancellationToken ct = default);

    Task<Result> UpdateClientAsync(
        UpdateClientDto dto, CancellationToken ct = default);

    Task<Result<string>> DeactivateClientAsync(
        int id, CancellationToken ct = default);
}