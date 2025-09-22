using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.UseCases.Reservations;

public class CreateReservationUseCase(
    IClientRepository clientRepository,
    ITableRepository tableRepository,
    IReservationRepository reservationRepository,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IReservationService reservationService,
    IPricingService pricingService)
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IPricingService _pricingService = pricingService;
    private readonly IReservationService _reservationService = reservationService;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<ReservationDto>> ExecuteAsync(
        CreateReservationDto dto, CancellationToken ct = default)
    {
        if (_currentUserService.UserId is not { } userId)
            return Result.Failure<ReservationDto>("User not authenticated.", 401);

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure<ReservationDto>("User not found.", 404);

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
            new ClientDto(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Email,
                client.Phone,
                client.Status.ToString()),
            new TableDto(
                table.Id,
                table.Code,
                table.Capacity,
                table.Location,
                table.TableTypeId,
                table.Status.ToString()),
            r.Date,
            r.StartTime,
            r.EndTime,
            r.NumberOfGuests,
            r.BasePrice,
            r.TotalPrice,
            r.Status.ToString(),
            r.Notes,
            new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role.ToString(),
                user.Status.ToString())
        );

        return Result.Success(reservationDto);
    }
}