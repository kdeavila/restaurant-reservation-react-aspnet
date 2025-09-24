using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<Reservation>> GetReservationsAsync(int ClientId, CancellationToken ct = default);

    Task AddAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsForOthersClients(string email, int clientId, CancellationToken ct = default);
}