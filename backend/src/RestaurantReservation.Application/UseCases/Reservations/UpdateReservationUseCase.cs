using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.UseCases.Reservations;

public class UpdateReservationUseCase(
    IReservationRepository reservationRepository,
    IPricingService pricingService,
    IReservationService reservationService)
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IReservationService _reservationService = reservationService;
    private readonly IPricingService _pricingService = pricingService;

    public async Task<Result> ExecuteAsync(UpdateReservationDto dto, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null) return Result.Failure("Reservation not found.");

        if (dto.TableId.HasValue || dto.Date.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
        {
            var tableId = dto.TableId ?? reservation.TableId;
            reservation.TableId = dto.TableId ?? reservation.TableId;
            var date = dto.Date ?? reservation.Date;
            var startTime = dto.StartTime ?? reservation.StartTime;
            var endTime = dto.EndTime ?? reservation.EndTime;

            var isAvailable = await _reservationRepository.ExistsOverlappingReservationAsync
                (tableId, date, startTime, endTime, ct);
            if (!isAvailable) return Result.Failure("The selected table is not available at the specified time.");

            var priceResult = await _pricingService.CalculatePriceAsync(tableId, date, startTime, endTime, ct);
            if (priceResult.IsFailure) return Result.Failure(priceResult.Error);

            var (basePrice, totalPrice) = priceResult.Value;
            reservation.BasePrice = basePrice;
            reservation.TotalPrice = totalPrice;
        }

        reservation.TableId = dto.TableId ?? reservation.TableId;
        reservation.Date = dto.Date ?? reservation.Date;
        reservation.StartTime = dto.StartTime ?? reservation.StartTime;
        reservation.EndTime = dto.EndTime ?? reservation.EndTime;
        reservation.NumberOfGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;
        reservation.Notes = dto.Notes ?? reservation.Notes;
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ReservationStatus>(dto.Status, out var parsed))
            reservation.Status = parsed;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationService.UpdateReservationAsync(reservation, ct);
        return Result.Success();
    }
}