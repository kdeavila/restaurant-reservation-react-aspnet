using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.UseCases.Reservations;

public class UpdateReservationUseCase(
    IReservationRepository reservationRepository,
    IPricingService pricingService,
    IReservationService reservationService,
    ITableRepository tableRepository,
    ITableTypeRepository tableTypeRepository
)
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IReservationService _reservationService = reservationService;
    private readonly IPricingService _pricingService = pricingService;
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

    public async Task<Result<string>> ExecuteAsync(
        UpdateReservationDto dto,
        CancellationToken ct = default
    )
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null)
            return Result.Failure<string>("Reservation not found.", 404);

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Completed)
            return Result.Failure<string>(
                "Cannot modify cancelled or completed reservations.",
                400
            );

        var currentTable = await _tableRepository.GetByIdAsync(reservation.TableId, ct);
        if (currentTable is null)
            return Result.Failure<string>("Table not found.", 404);

        Table? newTable = null;
        if (dto.TableId.HasValue)
        {
            newTable = await _tableRepository.GetByIdAsync(dto.TableId.Value, ct);
            if (newTable is null || newTable.Status != TableStatus.Active)
                return Result.Failure<string>("Selected table is not available.", 404);

            var tableType = await _tableTypeRepository.GetByIdAsync(newTable.TableTypeId, ct);
            if (tableType is null || !tableType.IsActive)
                return Result.Failure<string>("Table type is not available.", 400);
        }

        var finalTable = newTable ?? currentTable;
        var finalGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;

        if (finalGuests > finalTable.Capacity)
            return Result.Failure<string>(
                $"Table capacity exceeded. Maximum: {finalTable.Capacity} guests.",
                400
            );

        decimal? newBasePrice = null;
        decimal? newTotalPrice = null;

        var shouldCheckAvailability =
            dto.TableId.HasValue
            || dto.Date.HasValue
            || dto.StartTime.HasValue
            || dto.EndTime.HasValue;

        if (shouldCheckAvailability)
        {
            var tableId = dto.TableId ?? reservation.TableId;
            var date = dto.Date ?? reservation.Date;
            var start = dto.StartTime ?? reservation.StartTime;
            var end = dto.EndTime ?? reservation.EndTime;

            var overlap = await _reservationRepository.ExistsOverlappingReservationAsync(
                tableId,
                date,
                start,
                end,
                reservation.Id,
                ct
            );

            if (overlap)
                return Result.Failure<string>(
                    "The selected table is not available at the specified time.",
                    409
                );

            var priceResult = await _pricingService.CalculatePriceAsync(
                tableId,
                date,
                start,
                end,
                ct
            );
            if (priceResult.IsFailure)
                return Result.Failure<string>(priceResult.Error, priceResult.StatusCode);

            (newBasePrice, newTotalPrice) = priceResult.Value;
        }

        if (!string.IsNullOrEmpty(dto.Status) && dto.Status != reservation.Status.ToString())
        {
            if (!IsValidStatusTransition(reservation.Status, dto.Status))
                return Result.Failure<string>(
                    $"Invalid status transition from {reservation.Status} to {dto.Status}.",
                    400
                );
        }

        var updatedResult = await _reservationService.UpdateAsync(
            dto,
            newBasePrice,
            newTotalPrice,
            ct
        );

        return updatedResult.IsFailure
            ? Result.Failure<string>(updatedResult.Error, updatedResult.StatusCode)
            : Result.Success<string>("Reservation updated successfully.");
    }

    private static bool IsValidStatusTransition(ReservationStatus currentStatus, string newStatus)
    {
        var validTransitions = new Dictionary<ReservationStatus, ReservationStatus[]>
        {
            [ReservationStatus.Completed] = [],
            [ReservationStatus.Cancelled] = [],
            [ReservationStatus.Pending] = [ReservationStatus.Confirmed],
            [ReservationStatus.Confirmed] = [ReservationStatus.Completed],
        };

        return Enum.TryParse<ReservationStatus>(newStatus, out var parsedStatus)
            && validTransitions[currentStatus].Contains(parsedStatus);
    }
}
