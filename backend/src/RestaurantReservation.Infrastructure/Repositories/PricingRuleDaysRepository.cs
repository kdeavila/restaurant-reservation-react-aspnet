using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class PricingRuleDaysRepository(RestaurantReservationDbContext context) : IPricingRuleDaysRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<PricingRuleDays?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.PricingRuleDays
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<PricingRuleDays>> GetByPricingRuleIdAsync(
        int pricingRuleId,
        CancellationToken ct = default
    )
        => await _context.PricingRuleDays
            .Where(prd => prd.PricingRuleId == pricingRuleId)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<PricingRuleDays> dayEntities, CancellationToken ct = default)
    {
        await _context.PricingRuleDays.AddRangeAsync(dayEntities, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByPricingRuleIdAsync(int pricingRuleId, CancellationToken ct = default)
    {
        var existing = await _context.PricingRuleDays
            .Where(prd => prd.PricingRuleId == pricingRuleId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _context.PricingRuleDays.RemoveRange(existing);
            await _context.SaveChangesAsync(ct);
        }
    }


    public async Task<IEnumerable<PricingRuleDays>> GetByRuleIdAsync(int pricingRuleId, CancellationToken ct = default)
        => await _context.PricingRuleDays
            .AsNoTracking()
            .Where(d => d.PricingRuleId == pricingRuleId)
            .ToListAsync(ct);

    public async Task AddAsync(PricingRuleDays pricingRuleDays, CancellationToken ct = default)
    {
        await _context.PricingRuleDays.AddAsync(pricingRuleDays, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var day = await _context.PricingRuleDays.FindAsync([id], ct);
        if (day is null) return;

        _context.PricingRuleDays.Remove(day);
        await _context.SaveChangesAsync(ct);
    }
}