using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class ClientRepository(RestaurantReservationDbContext context) : IClientRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Clients
            .Include(c => c.Reservations)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Clients.FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
        => await _context.Clients
            .Include(c => c.Reservations)
            .ToListAsync(ct);

    public async Task<IEnumerable<Reservation>> GetReservationsAsync(int clientId, CancellationToken ct = default)
        => await _context.Reservations
            .AsNoTracking()
            .Where(r => r.ClientId == clientId)
            .OrderByDescending(r => r.Date).ThenBy(r => r.StartTime)
            .ToListAsync(ct);

    public async Task AddAsync(Client client, CancellationToken ct = default)
    {
        await _context.Clients.AddAsync(client, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var client = await _context.Clients.FindAsync([id], ct);
        if (client is null) return;

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _context.Clients.AnyAsync(c => c.Email == email, ct);

    public async Task<bool> EmailExistsForOthersClients(string email, int clientId, CancellationToken ct = default)
        => await _context.Clients.AnyAsync(c => c.Email == email && c.Id != clientId, ct);
}