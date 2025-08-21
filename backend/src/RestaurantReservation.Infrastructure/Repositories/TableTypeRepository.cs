using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class TableTypeRepository(RestaurantReservationDbContext context) : ITableTypeRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<TableType?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.TableTypes.FindAsync(id, ct);

    public async Task<IEnumerable<TableType>> GetAllAsync(CancellationToken ct = default)
        => await _context.TableTypes
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(TableType tableType, CancellationToken ct = default)
    {
        await _context.TableTypes.AddAsync(tableType, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TableType tableType, CancellationToken ct = default)
    {
        _context.TableTypes.Update(tableType);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var tableType = await _context.TableTypes.FindAsync(id, ct);
        if (tableType is null) return;

        _context.TableTypes.Remove(tableType);
        await _context.SaveChangesAsync(ct);
    }
}