using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class ReservationRepository(RestaurantReservationDbContext context) : IReservationRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default)
        => await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Table)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<Reservation>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
        => await _context.Reservations
            .Where(r => r.ClientId == clientId)
            .Include(r => r.Table)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<Reservation>> GetByTableIdAsync(int tableId, CancellationToken ct = default)
        => await _context.Reservations
            .Where(r => r.TableId == tableId)
            .Include(r => r.Client)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<bool> ExistsOverlappingReservationAsync(int tableId, DateTime date, TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken ct = default)
        => await _context.Reservations
            .AnyAsync(r =>
                    r.TableId == tableId &&
                    r.Date == date.Date && (
                        (startTime >= r.StartTime && startTime < r.EndTime) ||
                        (endTime > r.StartTime && endTime <= r.EndTime) ||
                        (startTime <= r.StartTime && endTime >= r.EndTime)
                    ),
                ct
            );

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        await _context.Reservations.AddAsync(reservation, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken ct = default)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _context.Reservations.FindAsync([id], ct);
        if (reservation is null) return;

        _context.Remove(reservation);
        await _context.SaveChangesAsync(ct);
    }
}