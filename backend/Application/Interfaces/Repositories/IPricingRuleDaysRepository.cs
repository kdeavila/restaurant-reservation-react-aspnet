using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IPricingRuleDaysRepository
{
    Task<PricingRuleDays?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IEnumerable<PricingRuleDays>> GetByPricingRuleIdsAsync(
        IEnumerable<int> ruleIds, CancellationToken ct = default);

    Task<IEnumerable<PricingRuleDays>> GetByPricingRuleIdAsync(int pricingRuleId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PricingRuleDays> dayEntities, CancellationToken ct = default);
    Task DeleteByPricingRuleIdAsync(int pricingRuleId, CancellationToken ct = default);
    Task AddAsync(PricingRuleDays pricingRuleDays, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}