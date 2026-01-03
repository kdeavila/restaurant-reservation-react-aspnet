using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class TableRepository(RestaurantReservationDbContext context) : ITableRepository
{
   private readonly RestaurantReservationDbContext _context = context;

   public IQueryable<Table> Query()
       => _context.Tables
       .Include(t => t.TableType)
       .AsNoTracking();

   public async Task<Table?> GetByIdAsync(int id, CancellationToken ct = default)
       => await _context.Tables
           .Include(t => t.TableType)
           .AsNoTracking()
           .FirstOrDefaultAsync(t => t.Id == id, ct);

   public async Task<IEnumerable<Table>> GetByTableTypeIdAsync(int tableTypeId, CancellationToken ct = default)
       => await _context.Tables
           .Where(t => t.TableTypeId == tableTypeId)
           .AsNoTracking()
           .ToListAsync(ct);

   public async Task<Dictionary<int, int>> GetTableCountsByTableTypeIdsAsync(IEnumerable<int> tableTypeIds,
       CancellationToken ct = default)
       => await _context.Tables
           .Where(t => tableTypeIds.Contains(t.TableTypeId))
           .GroupBy(t => t.TableTypeId)
           .Select(g => new { TableTypeId = g.Key, Count = g.Count() })
           .ToDictionaryAsync(x => x.TableTypeId, x => x.Count, ct);

   public async Task<IEnumerable<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan startTime,
       TimeSpan endTime, int capacity, CancellationToken ct = default)
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
      var table = await _context.Tables.FindAsync([id], ct);
      if (table is null) return;

      _context.Tables.Remove(table);
      await _context.SaveChangesAsync(ct);
   }
}