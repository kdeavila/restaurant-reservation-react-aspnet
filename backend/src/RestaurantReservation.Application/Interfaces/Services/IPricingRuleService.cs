using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IPricingRuleService
{
    Task<Result<PricingRule>> CreatePricingRuleAsync(CreatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result<PricingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<PricingRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result> UpdatePricingRuleAsync(UpdatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result> DeletePricingRuleAsync(int id, CancellationToken ct = default);
}