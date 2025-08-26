using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Common.Helpers;

public class PricingCalculator(
    ITableRepository tableRepository,
    ITableTypeRepository tableTypeRepository,
    IPricingRuleRepository pricingRuleRepository)
{
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;
    private readonly IPricingRuleRepository _pricingRuleRepository = pricingRuleRepository;

    // Calculate base and total price applying surcharges from pricing rules
    public async Task<(decimal BasePrice, decimal TotalPrice)> CalculatePriceAsync(
        int tableId, DateTime date, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(tableId, ct);
        if (table is null) throw new InvalidOperationException("Invalid table");

        var tableType = await _tableTypeRepository.GetByIdAsync(table.TableTypeId, ct);
        if (tableType is null) throw new InvalidOperationException("Invalid tableType");

        var hours = (decimal)(endTime - startTime).TotalHours;
        var basePrice = tableType.BasePricePerHour * hours;

        var rules = await _pricingRuleRepository.GetActiveByTableTypeWithDaysAsync(
            tableType.Id, date, startTime, endTime, ct);

        var reservationDay = (DaysOfWeek)(int)date.DayOfWeek;

        var applicableRules = rules
            .Where(r => r.PricingRuleDays.Any(d =>
                d.DayOfWeek == reservationDay));

        var totalSurcharge = applicableRules
            .Select(r => basePrice * (r.SurchargePercentage / 100m))
            .Sum();
        
        var totalPrice = basePrice + totalSurcharge;
        return (basePrice, totalPrice);
    }
}