using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.PricingRule;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IPricingRuleService
{
    Task<Result<PricingRuleDto>> CreatePricingRuleAsync(CreatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result<PricingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PricingRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> UpdatePricingRuleAsync(UpdatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result> DeletePricingRuleAsync(int id, CancellationToken ct = default);
}