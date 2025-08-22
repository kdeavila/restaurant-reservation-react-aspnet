using RestaurantReservation.Application.DTOs.Client;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IClientService
{
    Task<ClientDto?> CreateClientAsync(CreateClientDto dto, CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateClientAsync(UpdateClientDto dto, CancellationToken ct = default);
    Task<bool> DeleteClientAsync(int id, CancellationToken ct = default);
}