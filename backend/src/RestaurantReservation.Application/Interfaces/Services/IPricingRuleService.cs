using RestaurantReservation.Application.DTOs.PricingRule;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IPricingRuleService
{
    Task<PricingRuleDto?> CreatePricingRuleAsync(CreatePricingRuleDto dto, CancellationToken ct = default);
    Task<PricingRuleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PricingRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdatePricingRuleAsync(UpdatePricingRuleDto dto, CancellationToken ct = default);
    Task<bool> DeletePricingRuleAsync(int id, CancellationToken ct = default);
}