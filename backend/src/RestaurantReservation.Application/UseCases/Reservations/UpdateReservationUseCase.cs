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
        if (reservation is null) return Result.Failure("Reservation not found.", 404);

        if (dto.TableId.HasValue || dto.Date.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
        {
            var tableId = dto.TableId ?? reservation.TableId;
            var date = dto.Date ?? reservation.Date;
            var start = dto.StartTime ?? reservation.StartTime;
            var end = dto.EndTime ?? reservation.EndTime;

            var overlap = await _reservationRepository.ExistsOverlappingReservationAsync
                (tableId, date, start, end, ct);

            if (overlap)
                return Result.Failure("The selected table is not available at the specified time.", 409);

            var priceResult = await _pricingService.CalculatePriceAsync(tableId, date, start, end, ct);

            if (priceResult.IsFailure)
                return Result.Failure(priceResult.Error, priceResult.StatusCode);

            var (basePrice, totalPrice) = priceResult.Value;

            var result = await _reservationService.UpdateReservationAsync(dto, basePrice, totalPrice, ct);
            return result.IsFailure ? Result.Failure("Failed to update reservation.", 400) : Result.Success();
        }
        else
        {
            var result = await _reservationService.UpdateReservationAsync(dto, null, null, ct);
            return result.IsFailure ? Result.Failure("Failed to update reservation.", 400) : Result.Success();
        }
    }
}