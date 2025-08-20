using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IPricingRuleRepository
{
    Task<IReadOnlyList<PricingRule>> GetActiveRulesAsync(
        DateTime date, TimeSpan start, TimeSpan end, int? tableTypeId);
}