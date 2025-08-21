using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class TableRepository(RestaurantReservationDbContext context) : ITableRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<Table?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Tables
            .Include(t => t.TableType)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IEnumerable<Table>> GetAllAsync(CancellationToken ct = default)
        => await _context.Tables
            .Include(t => t.TableType)
            .ToListAsync(ct);

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, DateTime startTime, DateTime endTime,
        int capacity, CancellationToken ct = default)
        => await _context.Tables
            .Include(t => t.TableType)
            .Where(t => t.Capacity >= capacity &&
                        !_context.Reservations.Any(r =>
                            r.TableId == t.Id &&
                            r.Date == date &&
                            ((r.StartTime < endTime && r.EndTime > startTime))
                        )
            )
            .ToListAsync(ct);

    public async Task AddAsync(Table table, CancellationToken ct = default)
    {
        await _context.Tables.AddAsync(table, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Table table, CancellationToken ct = default)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var table = await _context.Tables.FindAsync(id, ct);
        if (table is null) return;

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync(ct);
    }
}