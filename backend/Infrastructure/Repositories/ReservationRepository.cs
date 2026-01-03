using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class ReservationRepository(RestaurantReservationDbContext context) : IReservationRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public IQueryable<Reservation> Query()
            => _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Table)
                .Include(r => r.User)
                .AsNoTracking();

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Table)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

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

    public async Task<bool> ExistsOverlappingReservationAsync(
        int tableId, DateTime date,
        TimeSpan startTime, TimeSpan endTime,
        int? exclusionReservationId = null, CancellationToken ct = default)
    {
        var query = _context.Reservations
            .Where(r => r.TableId == tableId &&
                        r.Date == date &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Completed &&
                        (
                            (startTime >= r.StartTime && startTime < r.EndTime) ||
                            (endTime > r.StartTime && endTime <= r.EndTime) ||
                            (startTime <= r.StartTime && endTime >= r.EndTime)
                        ));

        if (exclusionReservationId.HasValue)
            query = query.Where(r => r.Id != exclusionReservationId.Value);

        return await query.AnyAsync(ct);
    }

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
}