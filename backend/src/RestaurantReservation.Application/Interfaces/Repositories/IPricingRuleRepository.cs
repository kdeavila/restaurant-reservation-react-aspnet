using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IPricingRuleRepository
{
    Task<PricingRule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PricingRule>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<PricingRule>> GetActiveRulesAsync(CancellationToken ct = default);
    Task AddAsync(PricingRule rule, CancellationToken ct = default);
    Task UpdateAsync(PricingRule rule, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}