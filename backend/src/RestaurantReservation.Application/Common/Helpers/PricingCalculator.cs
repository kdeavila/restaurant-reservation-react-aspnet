using RestaurantReservation.Application.Interfaces.Repositories;

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
        ;

        var tableType = await _tableTypeRepository.GetByIdAsync(table.TableTypeId, ct);
        if (tableType is null) throw new InvalidOperationException("Invalid tableType");

        var hours = (decimal)(endTime - startTime).TotalHours;
        var basePrice = tableType.BasePricePerHour * hours;

        var rules = await _pricingRuleRepository.GetActiveByTableTypeWithDaysAsync(
            tableType.Id, date, startTime, endTime, ct);

        var reservationDayOfWeek = (int)date.DayOfWeek;

        var applicableRules = rules
            .Where(rule => rule.PricingRuleDays.Any(d =>
                (int)d.DayOfWeek == reservationDayOfWeek
            ))
            .ToList();

        var totalPrice = basePrice;
        foreach (var rule in applicableRules)
            totalPrice += basePrice * (rule.SurchargePercentage / 100m);

        return (basePrice, totalPrice);
    }
}