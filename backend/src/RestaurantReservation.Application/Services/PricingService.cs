using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.Application.Services;

public class PricingService(
    ITableRepository tableRepository,
    ITableTypeRepository tableTypeRepository,
    IPricingRuleRepository pricingRuleRepository)
    : IPricingService
{
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;
    private readonly IPricingRuleRepository _pricingRuleRepository = pricingRuleRepository;

    public async Task<Result<(decimal BasePrice, decimal TotalPrice)>> CalculatePriceAsync(
        int tableId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(tableId, ct);
        if (table is null) return Result.Failure<(decimal, decimal)>("Table not found", 404);

        var tableType = await _tableTypeRepository.GetByIdAsync(table.TableTypeId, ct);
        if (tableType is null) return Result.Failure<(decimal, decimal)>("Table type not found", 404);

        var hours = (decimal)(endTime - startTime).TotalHours;
        var basePrice = tableType.BasePricePerHour * hours;

        var applicableRules =
            await _pricingRuleRepository.GetApplicableRulesAsync(table.TableTypeId, date, startTime, endTime, ct);

        var totalPrice = applicableRules
            .Aggregate(basePrice, (current, rule) => current * (1 + (rule.SurchargePercentage / 100m)));

        return Result.Success((basePrice, totalPrice));
    }
}