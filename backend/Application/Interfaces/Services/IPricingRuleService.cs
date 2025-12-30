using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IPricingRuleService
{
    Task<Result<PricingRule>> CreateAsync(CreatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result<PricingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(IEnumerable<PricingRuleDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        PricingRuleQueryParams queryParams, CancellationToken ct = default);

    Task<Result> UpdateAsync(UpdatePricingRuleDto dto, CancellationToken ct = default);
    Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default);
}