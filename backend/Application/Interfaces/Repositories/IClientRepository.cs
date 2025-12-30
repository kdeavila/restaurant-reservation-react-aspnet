using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IClientRepository
{
    IQueryable<Client> Query();
    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Reservation>> GetReservationsAsync(int ClientId, CancellationToken ct = default);

    Task AddAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsForOthersClients(string email, int clientId, CancellationToken ct = default);
}