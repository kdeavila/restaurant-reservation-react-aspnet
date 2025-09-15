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
    ICurrentUserService currentUserService,
    IReservationService reservationService,
    IPricingService pricingService)
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IPricingService _pricingService = pricingService;
    private readonly IReservationService _reservationService = reservationService;

    public async Task<Result<ReservationDto>> ExecuteAsync(
        CreateReservationDto dto, CancellationToken ct = default)
    {
        if (_currentUserService.UserId is not int userId)
            return Result.Failure<ReservationDto>("User not authenticated.", 401);

        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        if (client is null || client.Status != ClientStatus.Active)
            return Result.Failure<ReservationDto>("Client not found or inactive.", 404);

        var table = await _tableRepository.GetByIdAsync(dto.TableId, ct);
        if (table is null || table.Status != TableStatus.Active)
            return Result.Failure<ReservationDto>("Table not found or inactive.", 404);

        if (dto.NumberOfGuests > table.Capacity)
            return Result.Failure<ReservationDto>
                ("The number of guests exceeds the table's capacity", 400);

        var overlap =
            await _reservationRepository.ExistsOverlappingReservationAsync
                (dto.TableId, dto.Date, dto.StartTime, dto.EndTime, ct);
        if (overlap)
            return Result.Failure<ReservationDto>
                ("Table is already booked for the selected time.", 409);

        var priceResult = await _pricingService.CalculatePriceAsync(
            dto.TableId,
            dto.Date,
            dto.StartTime,
            dto.EndTime,
            ct
        );
        if (priceResult.IsFailure)
            return Result.Failure<ReservationDto>(priceResult.Error, priceResult.StatusCode);
        var (basePrice, totalPrice) = priceResult.Value;

        var reservation = await
            _reservationService.CreateReservationAsync(dto, userId, basePrice, totalPrice, ct);

        if (reservation.IsFailure)
            return Result.Failure<ReservationDto>(reservation.Error, priceResult.StatusCode);

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
            r.Notes,
            r.CreatedByUserId
        );

        return Result.Success(reservationDto);
    }
}