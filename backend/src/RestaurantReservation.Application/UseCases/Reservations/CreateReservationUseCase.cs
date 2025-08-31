using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.UseCases.Reservations;

public class CreateReservationUseCase(
    IClientRepository clientRepository,
    ITableRepository tableRepository,
    IReservationRepository reservationRepository,
    IReservationService reservationService,
    IPricingService pricingService)
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IPricingService _pricingService = pricingService;
    private readonly IReservationService _reservationService = reservationService;

    public async Task<Result<ReservationDto>> ExecuteAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        if (client is null || client.Status != ClientStatus.Active)
            return Result.Failure<ReservationDto>("Client not found or inactive.");

        var table = await _tableRepository.GetByIdAsync(dto.TableId, ct);
        if (table is null || table.Status != TableStatus.Active)
            return Result.Failure<ReservationDto>("Table not found or inactive.");

        var overlap =
            await _reservationRepository.ExistsOverlappingReservationAsync(dto.TableId, dto.Date, dto.StartTime,
                dto.EndTime, ct);
        if (overlap) return Result.Failure<ReservationDto>("Table is already booked for the selected time.");

        var priceResult = await _pricingService.CalculatePriceAsync(
            dto.TableId,
            dto.Date,
            dto.StartTime,
            dto.EndTime,
            ct
        );
        if (priceResult.IsFailure)
            return Result.Failure<ReservationDto>(priceResult.Error);
        var (basePrice, totalPrice) = priceResult.Value;

        var reservation = await
            _reservationService.CreateReservationAsync(dto, basePrice, totalPrice, ct);

        if (reservation.IsFailure)
            return Result.Failure<ReservationDto>(reservation.Error);

        var r = reservation.Value;
        var reservationDto = new ReservationDto(
            r.Id,
            r.ClientId,
            $"{client.FirstName} {client.LastName}",
            r.TableId,
            table.Code,
            r.Date,
            r.StartTime,
            r.EndTime,
            r.NumberOfGuests,
            r.BasePrice,
            r.TotalPrice,
            r.Status.ToString(),
            r.Notes
        );

        return Result.Success(reservationDto);
    }
}