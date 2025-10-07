using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IPricingRuleRepository
{
    IQueryable<PricingRule> Query();
    Task<PricingRule?> GetByIdAsync(int id, CancellationToken ct = default);
    // Task<IEnumerable<PricingRule>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<PricingRule>> GetApplicableRulesAsync(
        int tableTypeId, DateTime date, TimeSpan startTime, TimeSpan endTime, CancellationToken ct = default);
    Task AddAsync(PricingRule rule, CancellationToken ct = default);
    Task UpdateAsync(PricingRule rule, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}