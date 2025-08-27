namespace RestaurantReservation.Application.Interfaces.Services;

public interface IPricingService
{
    Task<(decimal BasePrice, decimal TotalPrice)> CalculatePriceAsync(
        int tableId, DateTime date, TimeSpan startTime, TimeSpan endTime, CancellationToken ct = default);
}