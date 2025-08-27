using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class PricingRuleRepository(RestaurantReservationDbContext context) : IPricingRuleRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<PricingRule?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.PricingRules
            .Include(r => r.PricingRuleDays)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<PricingRule>> GetAllAsync(CancellationToken ct = default)
        => await _context.PricingRules
            .Include(r => r.PricingRuleDays)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<PricingRule>> GetActiveByTableTypeWithDaysAsync(
        int tableTypeId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken ct = default
    )
        => await _context.PricingRules
            .Include(r => r.PricingRuleDays)
            .Where(r =>
                r.TableTypeId == tableTypeId &&
                r.IsActive &&
                date.Date >= r.StartDate.Date &&
                date.Date <= r.EndDate.Date &&
                endTime > r.StartTime &&
                startTime < r.EndTime
            )
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<PricingRule>> GetActiveRulesAsync(CancellationToken ct = default)
        => await _context.PricingRules
            .Include(r => r.PricingRuleDays)
            .AsNoTracking()
            .Where(r => r.IsActive &&
                        (r.StartDate <= DateTime.UtcNow && r.EndDate >= DateTime.UtcNow))
            .ToListAsync(ct);

    public async Task AddAsync(PricingRule rule, CancellationToken ct = default)
    {
        await _context.PricingRules.AddAsync(rule, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PricingRule rule, CancellationToken ct = default)
    {
        _context.PricingRules.Update(rule);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var rule = await _context.PricingRules.FindAsync([id], ct);
        if (rule is null) return;

        _context.PricingRules.Remove(rule);
        await _context.SaveChangesAsync(ct);
    }
}