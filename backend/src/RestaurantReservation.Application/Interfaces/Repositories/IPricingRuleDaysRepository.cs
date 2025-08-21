using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IPricingRuleDaysRepository
{
    Task<PricingRuleDays?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PricingRuleDays>> GetByRuleIdAsync(int pricingRuleId, CancellationToken ct = default);
    Task AddAsync(PricingRuleDays pricingRuleDays, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}